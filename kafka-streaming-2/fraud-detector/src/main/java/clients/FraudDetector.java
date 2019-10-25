package clients;

import java.time.Duration;
import java.util.Arrays;
import java.util.Properties;
import org.apache.kafka.clients.consumer.ConsumerRecord;
import org.apache.kafka.clients.consumer.ConsumerRecords;
import org.apache.kafka.clients.consumer.KafkaConsumer;
import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.common.serialization.StringDeserializer;
import io.confluent.kafka.serializers.KafkaAvroDeserializer;
import io.confluent.kafka.serializers.KafkaAvroDeserializerConfig;
import messages.CCAuthorization;

public class FraudDetector {
    public static void main(String[] args) {
        System.out.println("*** Starting Fraud Detector ***");
        
        Properties settings = new Properties();
        settings.put(ConsumerConfig.GROUP_ID_CONFIG, "fraud-detector");
        settings.put(ConsumerConfig.BOOTSTRAP_SERVERS_CONFIG, "kafka:9092");
        settings.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");
        settings.put(ConsumerConfig.KEY_DESERIALIZER_CLASS_CONFIG, StringDeserializer.class);
        settings.put(ConsumerConfig.VALUE_DESERIALIZER_CLASS_CONFIG, KafkaAvroDeserializer.class);
        settings.put(KafkaAvroDeserializerConfig.SPECIFIC_AVRO_READER_CONFIG, "true");
        settings.put("schema.registry.url", "http://schema-registry:8081");

        KafkaConsumer<String, CCAuthorization> consumer = new KafkaConsumer<>(settings);
        try {
            consumer.subscribe(Arrays.asList("test-topic"));

            while (true) {
                ConsumerRecords<String, CCAuthorization> records = consumer.poll(Duration.ofMillis(100));
                for (ConsumerRecord<String, CCAuthorization> record : records)
                    System.out.printf("offset = %d, key = %s, value = %s\n", 
                        record.offset(), record.key().toString(), record.value().toString());
            }
        }
        finally{
            System.out.println("*** Ending Fraud Detector ***");
            consumer.close();
        }
    }
}