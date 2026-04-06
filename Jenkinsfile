pipeline {
    agent none

    stages {
        stage('Build Matrix') {
            matrix {
                axes {
                    axis {
                        name 'OS'
                        values 'windows', 'linux'
                    }
                    axis {
                        name 'BUILD_TYPE'
                        values 'debug', 'release'
                    }
                }

                agent { label "${OS}" }
                options {
                    throttle(['RamIntensiveJob'])
                }

                stages {
                    stage('Checkout') {
                        steps {
                            checkout scm
                            script {
                                if (isUnix()) {
                                    sh "git submodule sync --recursive"
                                    sh "git submodule update --init --recursive"
                                } else {
                                    bat "git submodule sync --recursive"
                                    bat "git submodule update --init --recursive"
                                }
                            }
                        }
                    }

                    stage('Get vcpkg commit') {
                        steps {
                            script {
                                def sha
                                if (isUnix()) {
                                    sha = sh(script: "cd vcpkg && git rev-parse HEAD", returnStdout: true).trim()
                                } else {
                                    sha = bat(script: "cd vcpkg && git rev-parse HEAD", returnStdout: true).trim()
                                }
                                env.VCPKG_SHA = sha
                            }
                        }
                    }

                    stage("Write auth.json") {
                        steps {
                            withCredentials([
                                string(credentialsId: 'STEAM_KEY', variable: 'STEAM_KEY')
                            ]) {
                                script {
                                    if (isUnix()) {
                                        sh "echo \"{\\\"steamApiKey\\\": \\\"$STEAM_KEY\\\"}\" > auth.json"
                                    } else {
                                        powershell '''
                                            $content = "{`"steamApiKey`": `"$env:STEAM_KEY`"}"
                                            Set-Content -Path auth.json -Value $content
                                        '''
                                    }
                                }
                            }
                        }
                    }

                    stage('Configure') {
                        steps {
                            script {
                                if (env.OS == 'windows') {
                                    bat """
                                        call \"C:\\Program Files\\Microsoft Visual Studio\\18\\Community\\VC\\Auxiliary\\Build\\vcvarsall.bat\" x64
                                        cmake --preset x64-${BUILD_TYPE}-win
                                    """
                                } else {
                                    sh "cmake --preset x64-${BUILD_TYPE}-linux"
                                }
                            }
                        }
                    }

                    stage('Build') {
                        steps {
                            script {
                                if (env.OS == 'windows') {
                                    bat """
                                        call \"C:\\Program Files\\Microsoft Visual Studio\\18\\Community\\VC\\Auxiliary\\Build\\vcvarsall.bat\" x64
                                        cmake --build out/build/x64-${BUILD_TYPE}-win
                                    """
                                } else {
                                    sh "cmake --build out/build/x64-${BUILD_TYPE}-linux"
                                }
                            }
                        }
                    }

                    stage('Archive') {
                        steps {
                            script {
                                if (env.OS == 'windows') {
                                    cleanCPPBuildDir("out/build/x64-${BUILD_TYPE}-win", "package-${BUILD_TYPE}-win", BUILD_TYPE == "debug")
                                    archiveArtifacts artifacts: "package-${BUILD_TYPE}-win/**", fingerprint: true
                                } else {
                                    cleanCPPBuildDir("out/build/x64-${BUILD_TYPE}-linux", "package-${BUILD_TYPE}-linux", BUILD_TYPE == "debug")
                                    archiveArtifacts artifacts: "package-${BUILD_TYPE}-linux/**", fingerprint: true
                                }
                            }
                        }
                    }
                }
            }
        }

        stage("code analysis") {
            parallel {
                stage("Code formatter") {
                    agent { label 'linux' }
                    steps {
                        script {
                            stage("Checkout") {
                                checkout scm
                            }
                            stage("Run formatter") {
                                sh """
                                    FILES=\$(find . -type f -regex '\\./\\(Packets\\|Persistence\\|tests\\)/.*\\.\\(h\\|cpp\\)\$')
                                    clang-format -i \$FILES main.cpp StaticHTTPPackets.cpp StaticWSPackets.cpp
                                """
                            }
                            stage("Create diff") {
                                sh """
                                    if ! git diff --quiet; then
                                        git diff > clang-format.patch
                                        echo \"Patch created, apply the patch from the artifacts section to fix\"
                                    else
                                        echo \"No changes required\"
                                    fi
                                """
                            }
                            stage("Upload diff if exists") {
                                if (fileExists('clang-format.patch')) {
                                    archiveArtifacts artifacts: 'clang-format.patch', fingerprint: true
                                }
                            }
                            stage("Fail if diff exists") {
                                if (fileExists('clang-format.patch')) {
                                    sh "exit 1"
                                }
                            }
                        }
                    }
                }

                stage("Code linter") {
                    agent { label 'linux' }
                    options {
                        throttle(['RamIntensiveJob'])
                    }
                    steps {
                        script {
                            stage("Checkout") {
                                checkout scm
                                sh "git submodule sync --recursive"
                                sh "git submodule update --init --recursive"
                            }
                            stage("Configure") {
                                sh "cmake --preset x64-debug-linux"
                            }
                            stage("Build protobuf files") {
                                sh "cmake --build out/build/x64-debug-linux --target generate_protos"
                            }
                            stage("Run linter") {
                                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE'){
                                sh """
                                    FILES=\$(find . -type f -regex '\\./\\(Packets\\|Persistence\\|tests\\)/.*\\.\\(h\\|cpp\\)\$')
                                    run-clang-tidy \$FILES main.cpp StaticHTTPPackets.cpp StaticWSPackets.cpp -fix -p out/build/x64-debug-linux -extra-arg=-Werror
                                """
                                }
                            }
                            stage("Create diff") {
                                sh """
                                    if ! git diff --quiet; then
                                        git diff > clang-format.patch
                                        echo \"Patch created, apply the patch from the artifacts section to fix\"
                                    else
                                        echo \"No changes required\"
                                    fi
                                """
                            }
                            stage("Upload diff if exists") {
                                if (fileExists('clang-format.patch')) {
                                    archiveArtifacts artifacts: 'clang-format.patch', fingerprint: true
                                }
                            }
                            stage("Fail if diff exists") {
                                if (fileExists('clang-format.patch')) {
                                    sh "exit 1"
                                }
                            }
                        }
                    }
                }
            }
        }

        stage("Run unit tests"){
            matrix {
                axes {
                    axis {
                        name 'OS'
                        values 'linux', 'windows'
                    }
                }
                agent { label '${OS}'}
                stages {
                    stage("Download artifact"){
                        steps {
                            copyArtifacts(
                                projectName: env.JOB_NAME,
                                selector: specific(env.BUILD_NUMBER),
                                filter: "package-release-${OS}/**",
                                target: 'pragmabackend'
                            )
                        }
                    }
                    stage("Run tests"){
                        steps {
                            script {
                                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE'){
                                    if(isUnix()){
                                        sh "pragmabackend/tests/tests"
                                    } else {
                                        bat "pragmabackend\\tests\\tests.exe"
                                    }
                                }
                            }
                        }
                    }
                    stage("Test environment cleanup"){
                        steps {
                            script {
                                if(isUnix()){
                                    sh "pkill -9 pragmabackend"
                                } else {
                                    bat "taskkill /f /im pragmabackend.exe"
                                }
                            }
                        }
                    }
                }
            }
        }

        /*stage("Build docker image"){
            agent { label 'host' }
            steps {
                script {
                    stage("Checkout"){
                        steps {
                            checkout scm
                        }
                    }
                    stage("Download x64-release-linux artifact"){
                        steps {
                            copyArtifacts(projectName: env.JOB_NAME, selector: specific(env.BUILD_NUMBER), filter: 'x64-release-linux/**', target: 'out/build/x64-release-linux/')
                        }
                    }
                    stage("Build image"){
                        steps {
                            script {
                                if(isUnix()){
                                    sh "docker build -t pragmabackend:latest ."
                                } else {
                                    bat "docker build -t pragmabackend:latest ."
                                }

                            }
                        }
                    }
                }
            }

        }*/
    }
    post {
        always {
            node('linux'){
                step([$class: 'GitHubCommitStatusSetter', contextSource: [$class: 'ManuallyEnteredCommitContextSource', context: 'jenkins/build-status'], reposSource: [$class: 'ManuallyEnteredRepositorySource', url: 'https://github.com/SpectreRevival/pragmabackend']])
            }
        }
    }
}
