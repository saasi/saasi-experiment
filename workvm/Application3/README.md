# Application 3



## Build with Docker
> This is just for testing locally. Not for running on Kubernetes.
```bash
build.sh
```

## Run with Docker
> This is just for testing locally. Not for running on Kubernetes.

*Must first build*

```bash
run.sh
```

## Scale with Docker
```bash
docker-compose -f docker-compose.yml -f docker-compose.override.yml scale cpu_microservice=3

```