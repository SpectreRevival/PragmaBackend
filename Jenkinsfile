pipeline {
    agent none

    stages {
		parallel {
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

					stage('Get vcpkg commit'){
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

					stage("Write auth.json"){
				steps {
							withCredentials([
								string(credentialsId: 'STEAM_KEY', variable: 'STEAM_KEY')
							]){
						script {
									if (isUnix()) {
								sh "echo \"{\\\"steamApiKey\\\": \\\"$STEAM_KEY\\\"}\" > auth.json"
									} else {
								powershell '''
										$content = "{`"steamApiKey`": `"$env:STEAM_KEY`"
									} "
									Set - Content - Path auth.json - Value $content
									'''
								}
							}
						}
					}
				}

				stage('Configure'){
			steps {
script {
							if (env.OS == 'windows') {
				bat "if exist out rmdir /s /q out"
				bat """
								call \"C:\\Program Files\\Microsoft Visual Studio\\18\\Community\\VC\\Auxiliary\\Build\\vcvarsall.bat\" x64
								cmake. --preset x64 - ${ BUILD_TYPE } -win
								"""
							} else {
                    sh """
								rm - rf out
								cmake. --preset x64 - ${ BUILD_TYPE } -linux
								"""
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
								cmake--build out / build / x64 - ${ BUILD_TYPE } -win
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
				def buildDir
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
	stage("Code formatter"){
		agent { label 'linux' }
		steps {
			script {
				stage("Checkout"){
					steps {
						checkout scm
					}
				}
				stage("Run formatter"){
					steps {
						sh """
							FILES=\$(find . -type f -regex '\\./\\(Packets\\|Persistence\\|tests\\)/.*\\.\\(h\\|cpp\\)\$')
							clang-format -i $FILES main.cpp StaticHTTPPackets.cpp StaticWSPackets.cpp
						"""
					}
				}
				stage("Create diff"){
					steps {
						sh """
							if ! git diff --quiet; then
								git diff > clang-format.patch
								echo \"Patch created, apply the patch from the artifacts section to fix\"
							else
								echo \"No changes required\"
							fi
						"""
					}
				}
				stage("Upload diff if exists"){
					when {
						expression { fileExists('clang-format.patch') }
					}
					steps {
						archiveArtifacts artifacts: 'clang-format.patch', fingerprint: true
					}
				}
				stage("Fail if diff exists"){
					when {
						expression { fileExists('clang-format.patch') }
					}
					steps {
						sh "exit 1"
					}
				}
			}
		}
	}
	stage("Code linter"){
		agent { label 'linux' }
		steps {
			script {
				stage("Checkout"){
					steps {
						checkout scm
						sh "git submodule sync --recursive"
						sh "git submodule update --init --recursive"
					}
				}
				stage("Configure"){
					steps {
						sh "cmake . --preset x64-debug-linux"
					}
				}
				stage("Build protobuf files"){
					steps {
						sh "cmake --build out/build/x64-debug-linux --target generate_protos"
					}
				}
				stage("Run linter"){
					sh """
						FILES=\$(find . -type f -regex '\\./\\(Packets\\|Persistence\\|tests\\)/.*\\.\\(h\\|cpp\\)\$')
						run-clang-tidy \$FILES main.cpp StaticHTTPPackets.cpp StaticWSPackets.cpp -fix -p out/build/x64-debug-linux -extra-arg=-Werror
					"""
				}
				stage("Create diff"){
					steps {
						sh """
							if ! git diff --quiet; then
								git diff > clang-tidy.patch
								echo \"Patch created, apply the patch from the artifacts section to fix\"
							else
								echo \"No changes required\"
							fi
						"""
					}
				}
				stage("Upload diff if exists"){
					when {
						expression { fileExists('clang-tidy.patch') }
					}
					steps {
						archiveArtifacts artifacts: 'clang-tidy.patch', fingerprint: true
					}
				}
				stage("Fail if diff exists"){
					when {
						expression { fileExists('clang-tidy.patch') }
					}
					steps {
						sh "exit 1"
					}
				}
			}
		}
	}
		}
}
}

