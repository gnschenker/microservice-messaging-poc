docker-compose up -d 

echo "Waiting for Kafka Connect to be ready..."
until $(curl --output /dev/null --silent --head --fail http://localhost:8083/); do
    printf '.'
    sleep 5
done

echo "Creating MQTT source connector..."
curl -X POST -H 'Content-Type: application/json' --data @connector.json http://localhost:8083/connectors
