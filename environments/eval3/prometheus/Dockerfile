FROM prom/prometheus:v2.0.0

COPY ./prometheus.yml /etc/prometheus/

ENTRYPOINT [ "/bin/prometheus" ]
CMD        [ "-config.file=/etc/prometheus/prometheus.yml", \
             "-storage.local.path=/prometheus"]