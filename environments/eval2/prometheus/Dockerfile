FROM prom/prometheus:v2.0.0

COPY ./prometheus.yml /etc/prometheus/
COPY ./rules /etc/prometheus/rules

ENTRYPOINT [ "/bin/prometheus" ]
CMD        [ "--config.file=/etc/prometheus/prometheus.yml" ]