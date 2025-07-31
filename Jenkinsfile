pipeline {
    agent any

    environment {
        DOCKER_CREDENTIALS_ID = '1d5350a5-b0ae-40b0-ae24-18e8b70721b8'  // Jenkins credentials ID for Docker Hub
        DOCKER_IMAGE_NAME = 'tipu247/aspnetcore6'
        DOCKER_IMAGE_TAG = 'latest'
        DOCKER_REGISTRY = 'https://index.docker.io/v1/'
        DEPLOY_PATH = "/var/www/html/inventory"
        SSH_USER = "deploy"
        DEPLOY_SERVER = "192.168.126.143"
    }

    stages {
        stage('Checkout') {
            steps {
                // Pull the code from the Git repository
                git branch: 'main', credentialsId: '3e711f9d-ea7a-49fd-9a5a-fd9c17a32f3d', url: 'https://github.com/tipusultandba/asp.net.enventory.git'
            }
        }

        stage('Build Docker Image') {
            steps {
                script {
                    // Build Docker image
                    docker.build("${DOCKER_IMAGE_NAME}:${DOCKER_IMAGE_TAG}")
                }
            }
        }

        stage('Login to Docker Hub') {
            steps {
                script {
                    // Log in to Docker Hub using Jenkins credentials
                    docker.withRegistry(DOCKER_REGISTRY, DOCKER_CREDENTIALS_ID) {
                        echo 'Logged in to Docker Hub successfully!'
                    }
                }
            }
        }

        stage('Push Docker Image') {
            steps {
                script {
                    // Push the Docker image to Docker Hub
                    docker.withRegistry(DOCKER_REGISTRY, DOCKER_CREDENTIALS_ID) {
                        docker.image("${DOCKER_IMAGE_NAME}:${DOCKER_IMAGE_TAG}").push()
                    }
                }
            }
        }
        stage('Approval') {
            steps {
                input "Do you approve this build?"
            }
        }
        stage('Deploy to Server') {
            steps {
                script {
                    // Use Jenkins credentials for Docker Hub login in the remote server
                    withCredentials([usernamePassword(credentialsId: DOCKER_CREDENTIALS_ID, usernameVariable: 'DOCKER_HUB_USER', passwordVariable: 'DOCKER_HUB_PASSWORD')]) {
                        // SSH into the server and deploy the app using Docker Compose
                        sh """
                       ssh ${SSH_USER}@${DEPLOY_SERVER} <<EOF 
echo "${DOCKER_HUB_PASSWORD}" | docker login -u "${DOCKER_HUB_USER}" --password-stdin
          docker pull "${DOCKER_IMAGE_NAME}:${DOCKER_IMAGE_TAG}" &&
          docker stop myapp || true &&
          docker rm myapp || true &&
          docker run -d --name myapp -p 8081:80 \
            -e ConnectionStrings__DbConn='Server=192.168.126.153;Database=inventorydb;User Id=Tipu;Password=Tipu@123;' \
            "${DOCKER_IMAGE_NAME}:${DOCKER_IMAGE_TAG}"
EOF
                        """
                    }
                }
            }
        }

        stage('Clean Up') {
            steps {
                script {
                    // Remove local Docker images to save space
                    sh "docker rmi ${DOCKER_IMAGE_NAME}:${DOCKER_IMAGE_TAG}"
                }
            }
        }
    }

    post {
        success {
            echo 'deployment successful!'
            mail to: 'tipu.brcp1@gmail.com',
                 from: 'tipu.idea2@gmail.com',
                 subject: "UAT Pipeline Success: ${currentBuild.fullDisplayName}",
                 body: "UAT deployment successful for build ${env.BUILD_NUMBER}.\nView console output at ${env.BUILD_URL}"
        }

        failure {
            echo 'deployment failed!'
            emailext attachLog: true,  // Attach the build log to the email
                    to: 'tipu.brcp1@gmail.com',
                    from: 'tipu.idea2@gmail.com',
                    subject: "UAT Pipeline Failure: ${currentBuild.fullDisplayName}",
                    body: "UAT deployment failed for build ${env.BUILD_NUMBER}.\nView console output at ${env.BUILD_URL}"
        }
        always {
            // Clean up workspace after build
            cleanWs()
        }
    }
}
