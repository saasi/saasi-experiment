# This file is intended for use with "docker stack"
# Might not work with docker-compose

version: '3.3'

services:
  business_web_1:
    image: regserv:5000/business_web
    labels:
      saasi.microservices.type: "business"
      saasi.group: 'workload'
 #     - "traefik.backend.healthcheck.path=/api/status"
 #     - "traefik.backend.healthcheck.interval=1s"
 #     - "traefik.backend.healthcheck.port=80"
 #     - "traefik.backend.circuitbreaker.expression=ResponseCodeRatio(400, 600, 0, 600)>0.1"
    deploy:
      labels:
        saasi.microservices.type: "business"
        traefik.backend: bms1
        traefik.docker.network: "eval2_appnet"
        traefik.port: 80
        traefik.frontend.rule: "Query: operationId={operationId:[0-1]+}"
        traefik.enable: "true"
#        traefik.backend.circuitbreaker.expression: "ResponseCodeRatio(400, 600, 0, 600)>0.1"
#        traefik.backend.healthcheck.path: "/api/status"
#        traefik.backend.healthcheck.interval: "1s"
#        traefik.backend.healthcheck.port: "80"
      replicas: 1
      placement:
        constraints: [node.role != manager]
      restart_policy:
        condition: any
      resources:
        limits:
          cpus: '0.40'
          memory: 512M
        reservations:
          cpus: '0.20'
          memory: 256M
    networks:
      - appnet
    logging:
      driver: "fluentd"
      options:
        fluentd-address: localhost:24224
        fluentd-async-connect: 
        tag: microservice.business.web1

  business_web_2:
    image: regserv:5000/business_web
    labels:
      saasi.microservices.type: "business"
      saasi.group: 'workload'
#      - "traefik.backend.healthcheck.path=/api/status"
#      - "traefik.backend.healthcheck.interval=1s"
#      - "traefik.backend.healthcheck.port=80"
#      - "traefik.backend.circuitbreaker.expression=ResponseCodeRatio(400, 600, 0, 600)>0.1"
    deploy:
      labels:
        saasi.microservices.type: "business"
        traefik.backend: bms2
        traefik.docker.network: "eval2_appnet"
        traefik.port: 80
        traefik.frontend.rule: "Query: operationId={operationId:[2-3]+}"
        traefik.enable: "true"
#        traefik.backend.circuitbreaker.expression: "ResponseCodeRatio(400, 600, 0, 600)>0.1"
#        traefik.backend.healthcheck.path: "/api/status"
#        traefik.backend.healthcheck.interval: "1s"
#        traefik.backend.healthcheck.port: "80"
      replicas: 1
      placement:
        constraints: [node.role != manager]
      restart_policy:
        condition: any
      resources:
        limits:
          cpus: '0.40'
          memory: 512M
        reservations:
          cpus: '0.20'
          memory: 256M
    networks:
      - appnet
    logging:
      driver: "fluentd"
      options:
        fluentd-address: localhost:24224
        fluentd-async-connect: 
        tag: microservice.business.web2

  business_web_3:
    image: regserv:5000/business_web
    labels:
      saasi.microservices.type: "business"
      saasi.group: 'workload'
#      - "traefik.backend.healthcheck.path=/api/status"
#      - "traefik.backend.healthcheck.interval=1s"
#      - "traefik.backend.healthcheck.port=80"
#      - "traefik.backend.circuitbreaker.expression=ResponseCodeRatio(400, 600, 0, 600)>0.1"
    deploy:
      labels:
        saasi.microservices.type: "business"
        traefik.backend: bms3
        traefik.docker.network: "eval2_appnet"
        traefik.port: 80
        traefik.frontend.rule: "Query: operationId={operationId:[4-5]+}"
        traefik.enable: "true"
#        traefik.backend.circuitbreaker.expression: "ResponseCodeRatio(400, 600, 0, 600)>0.1"
#        traefik.backend.healthcheck.path: "/api/status"
#        traefik.backend.healthcheck.interval: "1s"
#        traefik.backend.healthcheck.port: "80"
      replicas: 1
      placement:
        constraints: [node.role != manager]
      restart_policy:
        condition: any
      resources:
        limits:
          cpus: '0.40'
          memory: 512M
        reservations:
          cpus: '0.20'
          memory: 256M
    networks:
      - appnet
    logging:
      driver: "fluentd"
      options:
        fluentd-address: localhost:24224
        fluentd-async-connect: 
        tag: microservice.business.web3

  # Monitoring Components

  visualizer:
    image: dockersamples/visualizer:stable
    labels:
      saasi.group: 'controller'
    ports: 
     - "5009:8080"
    volumes:
     - "/var/run/docker.sock:/var/run/docker.sock"
    deploy:
      placement:
        constraints: [node.role == manager]
    networks:
     - appnet

  cadvisor:
    image: google/cadvisor:latest
    labels:
      saasi.group: 'controller'
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock:ro
      - /:/rootfs:ro
      - /var/run:/var/run
      - /sys:/sys:ro
      - /var/lib/docker/:/var/lib/docker:ro
    ports:
      - "8088:8080"
    deploy:
      mode: global
      restart_policy:
        condition: on-failure
    networks:
      - appnet

  prometheus:
    image: regserv:5000/prometheus:latest
    labels:
      saasi.group: 'controller'
    ports:
      - "9090:9090"
    deploy:
      placement:
        constraints: [node.role == manager]
      mode: replicated
      replicas: 1
      restart_policy:
        condition: on-failure
    networks:
      - appnet

  grafana:
    image: regserv:5000/grafana:latest
    labels:
      saasi.group: 'controller'
    ports:
      - "3000:3000"
    networks:
      - appnet
    environment:
      - GF_SECURITY_ADMIN_USER=${ADMIN_USER:-admin}
      - GF_SECURITY_ADMIN_PASSWORD=${ADMIN_PASSWORD:-admin}
      - GF_USERS_ALLOW_SIGN_UP=false
    deploy:
      mode: replicated
      replicas: 1
      placement:
        constraints:
          - node.role == manager
      resources:
        limits:
          memory: 128M
        reservations:
          memory: 64M

  swarm-exporter:
    image: regserv:5000/docker-swarm-exporter:latest
    labels:
      saasi.group: 'controller'
    deploy:
      mode: replicated
      replicas: 1
      placement:
        constraints:
          - node.role == manager
    ports:
      - "5051:5051"
    networks:
      - appnet
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock:ro

  autoscaler:
    image: regserv:5000/autoscaler:latest
    labels:
      saasi.group: 'controller'
    deploy:
      mode: replicated
      replicas: 1
      placement:
        constraints:
          - node.role == manager
    environment:
      - AUTOSCALER_PROM_HOST=http://tasks.prometheus:9090
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock:ro
    networks:
      - appnet
#    logging:
#      driver: "fluentd"
#      options:
#        fluentd-address: localhost:24224
#        fluentd-async-connect:
#        tag: controller.autoscaler

  fluentd:
    image: regserv:5000/fluentd:latest
    labels:
      saasi.group: 'controller'
    deploy: 
      mode: replicated
      replicas: 1
      placement:
        constraints:
          - node.role == manager
    ports:
      - "24224:24224"
      - "24224:24224/udp"
    networks:
      - appnet

  elasticsearch:
    image: elasticsearch
    labels:
      saasi.group: 'controller'
    deploy: 
      mode: replicated
      replicas: 1
      placement:
        constraints:
          - node.role == manager
    ports:
      - "9200:9200"
    networks:
      - appnet

  kibana:
    labels:
      saasi.group: 'controller'
    image: kibana
    deploy: 
      mode: replicated
      replicas: 1
      placement:
        constraints:
          - node.role == manager
    ports:
      - "5601:5601"
    networks:
      - appnet

  traefik:
    image: regserv:5000/traefik:latest
    command: traefik \
             --docker \
             --docker.swarmmode \
             --docker.domain=saasi \
             --docker.watch \
             --web
    ports:
      - "8080:8080"
      - "80:80"
    networks:
      - appnet
    deploy:
      mode: replicated
      placement:
        constraints:
          - node.role == manager
    volumes:
      - /var/run/docker.sock:/var/run/docker.sock:ro

networks: # Overlay network 
  appnet: # Network for the microservices
  controlnet: # Network for the controllers
