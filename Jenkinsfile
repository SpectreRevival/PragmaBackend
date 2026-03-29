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

                stages {

                    stage('Checkout') {
                        steps { 
				checkout scm
				script {
					if(isUnix()){
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
					if(isUnix()){
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
							if(isUnix()){
								sh "echo \"{\\\"steamApiKey\\\": \\\"${STEAM_KEY}\\\"}\" > auth.json"
							} else {
								bat "echo \"{\\\"steamApiKey\\\": \\\"${STEAM_KEY}\\\"}\" > auth.json"
							}
						}
					}
				}
			}

		    stage('Configure'){
			steps {
script {
	                if (env.OS == 'windows'){
				bat "if exist out rmdir /s /q out"
				bat "cmake . --preset x64-${BUILD_TYPE}-win"
                } else {
                    sh """
                        rm -rf out
                        cmake . --preset x64-${BUILD_TYPE}-linux
                    """
                }
		    }
}
}

                    stage('Build') {
                        steps {
                            script {
                                if (env.OS == 'windows') {
                                    bat "cmake --build out/build/x64-${BUILD_TYPE}-win"
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
				if(env.OS == 'windows'){
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
    }
}

