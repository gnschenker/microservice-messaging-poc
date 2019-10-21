#! /bin/bash

docker-compose up -d schema-registry
docker-compose up -d control-center

docker-compose exec kafka kafka-topics \
    --bootstrap-server kafka:9092 \
    --create \
    --topic test-topic \
    --partitions 3 \
    --replication-factor 1

echo "Topic 'test-topic' created."

docker-compose up -d publisher
docker-compose up -d consumer
