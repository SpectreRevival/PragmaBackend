pipeline {
    agent none

    stages {
        stage("Spell check") {
            agent { label 'linux' }
            steps {
                checkout scm
                sh 'codespell'
            }
        }

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
                                        sh "echo \"{\\\"steamApiKey\\\": \\\"\$STEAM_KEY\\\"}\" > auth.json"
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
                                        call "C:\\BuildTools\\VC\\Auxiliary\\Build\\vcvarsall.bat" x64
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
                                        call "C:\\BuildTools\\VC\\Auxiliary\\Build\\vcvarsall.bat" x64
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

                    stage("Stash") {
                        when {
                            expression { BUILD_TYPE == "release" }
                        }
                        steps {
                            script {
                                if (env.OS == 'windows') {
                                    stash name: 'winbuild', includes: 'package-release-win/**'
                                } else {
                                    stash name: 'linuxbuild', includes: 'package-release-linux/**'
                                }
                            }
                        }
                    }
                } // end matrix stages
            } // end matrix
        } // end Build Matrix

        stage("Post-matrix parallel tasks") {
            parallel {
                stage("Build docker server container") {
                    agent { label 'docker-linux' }
                    steps {
                        checkout scm
                        dir('out/build') {
                            sh "rm -rf x64-release-linux"
                            sh "rm -rf package-release-linux"
                            unstash 'linuxbuild'
                            sh "mv package-release-linux x64-release-linux"
                        }
                        sh "docker build -t pragmabackend:latest ."
                        sh "docker tag pragmabackend:latest ohmivr/pragmabackend:latest"
                        sh "docker push ohmivr/pragmabackend:latest"
                        sh "docker save pragmabackend:latest -o pragmabackend-docker.tar"
                        archiveArtifacts artifacts: 'pragmabackend-docker.tar', fingerprint: true
                        sh "rm pragmabackend-docker.tar"
                    }
                }

                stage("Code analysis") {
                    stages { // REQUIRED WRAPPER FOR NESTED PARALLEL
                        stage("Execution") {
                            parallel {
                                stage("Code formatter") {
                                    agent { label 'linux' }
                                    steps {
                                        script {
                                            checkout scm
                                            sh """
                                                FILES=\$(find . -type f -regex '\\./\\(Packets\\|Persistence\\|tests\\)/.*\\.\\(h\\|cpp\\)\$')
                                                clang-format -i \$FILES main.cpp StaticHTTPPackets.cpp StaticWSPackets.cpp
                                            """
                                            sh """
                                                if ! git diff --quiet; then
                                                    git diff > clang-format.patch
                                                    echo "Patch created, apply the patch from the artifacts section to fix"
                                                else
                                                    echo "No changes required"
                                                fi
                                            """
                                            if (fileExists('clang-format.patch')) {
                                                archiveArtifacts artifacts: 'clang-format.patch', fingerprint: true
                                                sh "rm clang-format.patch"
                                                error("Formatting changes required")
                                            }
                                        }
                                    }
                                }

                                stage("Code linter") {
                                    agent { label 'linux' }
                                    options { throttle(['RamIntensiveJob']) }
                                    steps {
                                        script {
                                            checkout scm
                                            sh "git submodule sync --recursive"
                                            sh "git submodule update --init --recursive"
                                            sh "cmake --preset x64-debug-linux"
                                            sh "cmake --build out/build/x64-debug-linux --target generate_protos"

                                            catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                                                sh """
                                                    FILES=\$(find . -type f -regex '\\./\\(Packets\\|Persistence\\|tests\\)/.*\\.\\(h\\|cpp\\)\$')
                                                    run-clang-tidy \$FILES main.cpp StaticHTTPPackets.cpp StaticWSPackets.cpp -fix -p out/build/x64-debug-linux -extra-arg=-Werror
                                                """
                                            }
                                            sh """
                                                if ! git diff --quiet; then
                                                    git diff > clang-tidy.patch
                                                    echo "Patch created, apply the patch from the artifacts section to fix"
                                                else
                                                    echo "No changes required"
                                                fi
                                            """
                                            if (fileExists('clang-tidy.patch')) {
                                                archiveArtifacts artifacts: 'clang-tidy.patch', fingerprint: true
                                                sh "rm clang-tidy.patch"
                                                error("Linter changes required")
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                stage("Run unit tests") {
                    stages { // REQUIRED WRAPPER FOR NESTED MATRIX
                        stage("Execution") {
                            matrix {
                                axes {
                                    axis {
                                        name 'OS'
                                        values 'linux', 'windows'
                                    }
                                }

                                agent { label "${OS}" }

                                stages {
                                    stage("Unstash") {
                                        steps {
                                            script {
                                                if (env.OS == 'windows') {
                                                    bat "if not exist testdir mkdir testdir"
                                                    dir('testdir') {
                                                        unstash 'winbuild'
                                                    }
                                                } else {
                                                    sh "mkdir -p testdir"
                                                    sh "rm -rf testdir/package-release-linux"
                                                    dir('testdir') {
                                                        unstash 'linuxbuild'
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    stage("Run tests") {
                                        steps {
                                            script {
                                                catchError(buildResult: 'FAILURE', stageResult: 'FAILURE') {
                                                    if (isUnix()) {
                                                        sh "testdir/package-release-linux/tests/tests"
                                                    } else {
                                                        bat "testdir\\package-release-win\\tests\\tests.exe"
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    stage("Test environment cleanup") {
                                        steps {
                                            script {
                                                if (isUnix()) {
                                                    sh "pkill -9 pragmabackend || true"
                                                } else {
                                                    bat "taskkill /f /im pragmabackend.exe || exit 0"
                                                }
                                            }
                                        }
                                    }
                                } // end matrix inner stages
                            } // end matrix
                        }
                    }
                }
            } // end parent parallel
        }
    } // end outer stages

    post {
        always {
            node('linux') {
                step([
                    $class: 'GitHubCommitStatusSetter',
                    contextSource: [$class: 'ManuallyEnteredCommitContextSource', context: 'jenkins/build-status'],
                    reposSource: [$class: 'ManuallyEnteredRepositorySource', url: 'https://github.com/SpectreRevival/pragmabackend']
                ])
            }
        }
    }
}