// FinSecure Pay - CI/CD pipeline
// Replace <dockerhub-user>, <your-sonar-org>, <your-project-key> and the NOTIFY address.
// FinSecure Pay - CI/CD pipeline (Task 4: core stages; security stages added in Task 5)
pipeline {
  agent any

  environment {
    IMAGE   = 'kate391/finsecure-pay'
    VERSION = "1.0.${env.BUILD_NUMBER}"
    CLUSTER = 'finsecure'
    REGION  = 'ap-southeast-1'
    NOTIFY  = 'khinthitthitmaw@gmail.com'
    DOTNET_CLI_TELEMETRY_OPTOUT = '1'
    DOTNET_NOLOGO = '1'
  }

  options {
    timestamps()
    disableConcurrentBuilds()
    buildDiscarder(logRotator(numToKeepStr: '20'))
  }

  stages {
    stage('Checkout') {
      steps { checkout scm }
    }

    stage('Build') {
      steps {
        sh 'dotnet restore'
        sh 'dotnet build -c Release --no-restore'
      }
    }

    stage('Test') {
      steps {
        sh 'dotnet test -c Release --no-build --logger "junit;LogFilePath=$WORKSPACE/test-results/junit.xml"'
      }
      post { always { junit 'test-results/*.xml' } }
    }

    stage('Package') {
      steps {
        sh 'dotnet publish src/FinSecurePay.Api/FinSecurePay.Api.csproj -c Release -o publish --no-build'
      }
    }

    stage('Container Build') {
      steps { sh 'docker build --build-arg APP_VERSION=$VERSION -t $IMAGE:$VERSION .' }
    }

    stage('Push') {
      steps {
        withCredentials([usernamePassword(credentialsId: 'dockerhub-creds',
                         usernameVariable: 'DH_USER', passwordVariable: 'DH_TOKEN')]) {
          sh 'echo "$DH_TOKEN" | docker login -u "$DH_USER" --password-stdin'
          sh 'docker push $IMAGE:$VERSION'
        }
      }
      post { always { sh 'docker logout || true' } }
    }

    stage('Deploy') {
      steps {
        sh '''
          aws eks update-kubeconfig --name $CLUSTER --region $REGION
          kubectl -n finsecure set image deployment/finsecure-pay app=$IMAGE:$VERSION
          kubectl -n finsecure annotate deployment/finsecure-pay \
            kubernetes.io/change-cause="Jenkins build $BUILD_NUMBER image $VERSION" --overwrite
          kubectl -n finsecure rollout status deployment/finsecure-pay --timeout=180s
        '''
      }
      post {
        failure { sh 'kubectl -n finsecure rollout undo deployment/finsecure-pay || true' }
      }
    }
  }

  post {
    success {
      emailext to: env.NOTIFY,
        subject: "SUCCESS: ${env.JOB_NAME} #${env.BUILD_NUMBER} deployed ${env.VERSION}",
        body: "Version ${env.VERSION} is live on EKS.\n${env.BUILD_URL}"
    }
    failure {
      emailext to: env.NOTIFY,
        subject: "FAILED: ${env.JOB_NAME} #${env.BUILD_NUMBER}",
        body: "The pipeline failed and nothing was deployed.\nConsole: ${env.BUILD_URL}console",
        attachLog: true
    }
  }
}