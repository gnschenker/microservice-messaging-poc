#! /bin/bash

docker-compose -f docker-compose-kafka.yml up -d schema-registry
docker-compose -f docker-compose-kafka.yml up -d control-center

docker-compose -f docker-compose-kafka.yml exec kafka kafka-topics \
    --bootstrap-server kafka:9092 \
    --create \
    --topic test-topic \
    --partitions 3 \
    --replication-factor 1

echo "Topic 'test-topic' created."

docker-compose -f docker-compose-kafka.yml up -d --build publisher
# docker-compose up -d consumer
