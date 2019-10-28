docker-compose up -d schema-registry
docker-compose up -d tools
docker-compose up -d control-center

echo "Creating the topics..."
docker-compose exec kafka kafka-topics \
    --bootstrap-server kafka:9092 \
    --create \
    --topic cc-authorizations \
    --partitions 3 \
    --replication-factor 1

docker-compose exec kafka kafka-topics \
    --bootstrap-server kafka:9092 \
    --create \
    --topic potential-fraud \
    --partitions 3 \
    --replication-factor 1

echo "Starting the CC Authorization provider..."
docker-compose up -d cc-validator
