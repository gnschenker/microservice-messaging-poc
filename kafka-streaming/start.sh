#! /bin/bash

docker-compose up -d connect

echo "Creating topic 'vehicle-positions'..."
docker-compose exec kafka kafka-topics \
    --bootstrap-server kafka:9092 \
    --create \
    --topic vehicle-positions \
    --partitions 3 \
    --replication-factor 1

echo "Waiting for Kafka Connect to be ready..."
until $(curl --output /dev/null --silent --head --fail http://localhost:8083/); do
    printf '.'
    sleep 5
done

echo "Creating MQTT source connector..."
curl -X POST -H 'Content-Type: application/json' --data @mqtt-source.json http://localhost:8083/connectors
