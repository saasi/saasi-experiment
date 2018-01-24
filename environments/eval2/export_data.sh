#!/bin/sh
echo $1
STYX=~/go/bin/styx
TIME_SPAN=$1
OUTPUT_DIR=$2

$STYX -d $TIME_SPAN 'bms_business_violation_total' > $OUTPUT_DIR/bms_business_violation_total.csv
$STYX -d $TIME_SPAN 'docker_swarm_service_target_replicas{service_name=~"\\w+_business_web_1"}' > $OUTPUT_DIR/scale_business_web_1.csv
$STYX -d $TIME_SPAN 'docker_swarm_service_target_replicas{service_name=~"\\w+_business_web_2"}' > $OUTPUT_DIR/scale_business_web_1.csv
$STYX -d $TIME_SPAN 'docker_swarm_service_target_replicas{service_name=~"\\w+_business_web_3"}' > $OUTPUT_DIR/scale_business_web_1.csv
$STYX -d $TIME_SPAN 'avg(rate(container_cpu_usage_seconds_total[1m])) by (container_label_com_docker_swarm_service_name) * 100' > $OUTPUT_DIR/cpu_usage.csv
$STYX -d $TIME_SPAN 'avg(container_memory_rss) by (container_label_com_docker_swarm_service_name)' > $OUTPUT_DIR/memory_usage.csv
$STYX -d $TIME_SPAN 'avg(rate(container_fs_writes_bytes_total[1m])) by (container_label_com_docker_swarm_service_name)' > $OUTPUT_DIR/io_write_usage.csv


docker run --rm -ti --net=host taskrabbit/elasticsearch-dump --input=http://localhost:9200/fluentd-*  --output=$ --type=data | gzip > $OUTPUT_DIR/logs.json.gz