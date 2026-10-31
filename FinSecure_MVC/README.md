# FinSecure Pay - Transaction Validation API

ASP.NET Core (.NET 10) Web API for the DevOps capstone: build, test, containerise,
deploy to Amazon EKS through Jenkins, with security gates, monitoring and alerting.

## Endpoints

| Method | Path | Purpose |
| --- | --- | --- |
| GET | `/health` | Service status and version |
| POST | `/validate` | Validates a transaction (200 approved, 422 rejected) |
| GET | `/health/live` | Kubernetes liveness probe |
| GET | `/health/ready` | Kubernetes readiness probe |
| GET | `/metrics` | Prometheus metrics |

## Validation rules

| # | Rule | Rejection reason |
| --- | --- | --- |
| 1 | TransactionId and AccountId are present | MISSING_FIELDS |
| 2 | AccountId matches `ACC` + 6 digits | INVALID_ACCOUNT_FORMAT |
| 3 | Amount is greater than 0 | NON_POSITIVE_AMOUNT |
| 4 | Amount does not exceed the limit (default 10,000) | AMOUNT_OVER_LIMIT |
| 5 | Currency is allowed (default SGD, USD, EUR) | UNSUPPORTED_CURRENCY |
| 6 | Merchant category is not GAMBLING or CRYPTO_ATM | BLOCKED_MERCHANT |

## Run locally

```bash
dotnet restore
dotnet build
dotnet test
dotnet run --project src/FinSecurePay.Api --urls http://localhost:8080
```

```bash
curl http://localhost:8080/health
curl -X POST http://localhost:8080/validate -H "Content-Type: application/json" \
  -d '{"transactionId":"TXN-1","accountId":"ACC123456","amount":250,"currency":"SGD","merchantCategory":"RETAIL"}'
```

## Docker

```bash
docker build --build-arg APP_VERSION=1.0.0 -t <dockerhub-user>/finsecure-pay:1.0.0 .
docker run -d --name fsp -p 8080:8080 <dockerhub-user>/finsecure-pay:1.0.0
curl http://localhost:8080/health

docker build --build-arg APP_VERSION=1.0.0 -t khinthitthitmaw@gmail.com/finsecure-pay:1.0.0 .
docker run -d --name fsp -p 8080:8080 khinthitthitmaw@gmail.com/finsecure-pay:1.0.0
```

## Repository layout

```text
FinSecurePay.sln
Dockerfile, .dockerignore, Jenkinsfile
src/FinSecurePay.Api/        ASP.NET Core Web API
tests/FinSecurePay.Tests/    xUnit tests
k8s/                         Namespace, ConfigMap, Deployment, Service
monitoring/                  Helm values, ServiceMonitor, alert rules
```

No secrets are stored in this repository. Pipeline credentials live in Jenkins
Credentials; runtime secrets are created with `kubectl create secret`.
