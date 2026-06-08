pipeline {
    agent none

    stages {
        stage("Compile backend"){
            matrix {
                axes {
                    axis {
                        name 'OS'
                        values 'windows', 'linux'
                    }
                    axis {
                        name 'CONFIGURATION'
                        values 'Debug', 'Release'
                    }
                }
                
                agent { label "${OS}" }

                stages {
                    stage("Checkout"){
                        steps {
                            checkout scm
                        }
                    }
                    stage("Download dependencies"){
                        steps {
                            script {
                                if(isUnix()){
                                    sh 'dotnet restore'
                                } else {
                                    bat 'dotnet restore'
                                }
                            }
                        }
                    }
                    stage("Build project"){
                        steps {
                            script {
                                if(isUnix()){
                                    sh "dotnet build --configuration ${CONFIGURATION} /m:1 /p:UseSharedCompilation=false /nodeReuse:false"
                                } else {
                                    bat "dotnet build --configuration ${CONFIGURATION} /m:1 /p:UseSharedCompilation=false /nodeReuse:false"
                                }
                            }
                        }
                    }
                    stage("Archive artifacts"){
                        steps {
                            script {
                                def artifactPath = "**/bin/Release/**"
                                archiveArtifacts artifacts: "${artifactPath}", allowEmptyArchive: true, fingerprint: true
                            }
                        }
                    }
                }
            }
        }
        stage("Compile backend docker image"){
            agent { label 'docker-linux' }
            steps {
                checkout scm
                dir("Docker"){
                    sh "docker build -t pragmabackend:latest -f Backend.dockerfile .."
                    sh "docker tag pragmabackend:latest registry.bgfamily.ca/pragmabackend:latest"
                    sh "docker push registry.bgfamily.ca/pragmabackend:latest"
                    sh "docker tag pragmabackend:latest ohmivr/pragmabackend:latest"
                    sh "docker push ohmivr/pragmabackend:latest"
                }
            }
        }
        stage("Compile database docker image"){
            agent { label 'docker-linux' }
            steps {
                checkout scm
                dir("Docker"){
                    sh "docker build -t pragmabackend-pgdb:latest -f Postgres.dockerfile ."
                    sh "docker tag pragmabackend-pgdb:latest registry.bgfamily.ca/pragmabackend-pgdb:latest"
                    sh "docker push registry.bgfamily.ca/pragmabackend-pgdb:latest"
                    sh "docker tag pragmabackend-pgdb:latest ohmivr/pragmabackend-pgdb:latest"
                    sh "docker push ohmivr/pragmabackend-pgdb:latest"
                }
            }
        }
        stage("Run tests"){
            matrix {
                axes {
                    axis {
                        name 'OS'
                        values 'linux'
                    }
                }
                agent { label "docker-${OS}" }
                stages {
                    stage("Checkout"){
                        steps {
                            checkout scm
                        }
                    }
                    stage("Download dependencies"){
                        steps {
                            script {
                                if(isUnix()){
                                    sh 'dotnet restore'
                                } else {
                                    bat 'dotnet restore'
                                }
                            }
                        }
                    }
                    stage("Build project"){
                        steps {
                            script {
                                if(isUnix()){
                                    sh 'dotnet build --configuration Release /m:1 /p:UseSharedCompilation=false /nodeReuse:false'
                                } else {
                                    bat 'dotnet build --configuration Release /m:1 /p:UseSharedCompilation=false /nodeReuse:false'
                                }
                            }
                        }
                    }
                    stage("Run database tests"){
                        steps {
                            script {
                                if(isUnix()){
                                    sh 'dotnet test --configuration Release --filter Name~TestDatabaseSync'
                                } else {
                                    bat 'dotnet test --configuration Release --filter Name~TestDatabaseSync'
                                }
                            }
                        }
                    }
                    stage("Run HTTP tests"){
                        steps {
                            script {
                                if(isUnix()){
                                    sh 'dotnet test --configuration Release --filter Name~RunHTTPTest'
                                } else {
                                    bat 'dotnet test --configuration Release --filter Name~RunHTTPTest'
                                }
                            }
                        }
                    }
                    stage("Run Websocket Tests"){
                        steps {
                            script {
                                if(isUnix()){
                                    sh 'dotnet test --configuration Release --filter Name~RunWebsocketTest'
                                } else {
                                    bat 'dotnet test --configuration Release --filter Name~RunWebsocketTest'
                                }
                            }
                        }
                    }
                }
            }
        }
    }

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
