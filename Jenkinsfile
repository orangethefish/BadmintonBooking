pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:8.0'
        }
    }
    
    environment {
        SOLUTION_NAME = 'BadmintonBooking'
        REPORTS_PATH = 'test-reports/build_${BUILD_NUMBER}'
        LINTER_REPORTS_PATH = 'linter-reports'
        DOCKER_CREDENTIALS = credentials('dockerhub-credentials-id')
        DOCKER_IMAGE_NAME = 'duyhoa2210/badminton-booking'
    }
    
    stages {
        stage('Build and Test') {
            steps {
                sh 'dotnet restore'
                sh 'dotnet build --no-restore --configuration Release'
                sh "dotnet test --no-build --configuration Release --test-adapter-path:. --logger:\"junit;LogFilePath=${REPORTS_PATH}/junit.xml\""
            }
        }
        
        stage('Lint the code') {
            steps {
                sh 'dotnet new tool-manifest || true'
                sh 'dotnet tool install JetBrains.ReSharper.GlobalTools || true'
                sh 'dotnet tool restore'
                sh "dotnet jb inspectcode ${SOLUTION_NAME}.sln --output=\"${LINTER_REPORTS_PATH}/jb-${BUILD_NUMBER}.xml\""
            }
        }
        
        stage('Docker Build') {
            when {
                branch 'main'  // Only build Docker image for the main branch
            }
            steps {
                sh "docker build -t ${DOCKER_IMAGE_NAME}:${BUILD_NUMBER} -t ${DOCKER_IMAGE_NAME}:latest ."
            }
        }
        
        stage('Docker Push') {
            when {
                branch 'main'  // Only push to Docker Hub for the main branch
            }
            steps {
                sh "echo ${DOCKER_CREDENTIALS_PSW} | docker login -u ${DOCKER_CREDENTIALS_USR} --password-stdin"
                sh "docker push ${DOCKER_IMAGE_NAME}:${BUILD_NUMBER}"
                sh "docker push ${DOCKER_IMAGE_NAME}:latest"
            }
        }
    }
    
    post {
       always {
               // Archive test and linting reports
               archiveArtifacts artifacts: 'test-reports/**,linter-reports/**', allowEmptyArchive: true
               junit 'test-reports/**/junit.xml'
               
               // Clean up Docker images after push
               sh "docker rmi ${DOCKER_IMAGE_NAME}:${BUILD_NUMBER} || true"
               sh "docker rmi ${DOCKER_IMAGE_NAME}:latest || true"
       }
   }

}
