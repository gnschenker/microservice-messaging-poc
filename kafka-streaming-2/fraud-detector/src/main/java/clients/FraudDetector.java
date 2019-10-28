package clients;

import java.util.Collections;
import java.util.Map;
import java.util.Properties;

import org.apache.kafka.clients.consumer.ConsumerConfig;
import org.apache.kafka.common.serialization.Serde;
import org.apache.kafka.common.serialization.Serdes;
import org.apache.kafka.streams.KafkaStreams;
import org.apache.kafka.streams.StreamsBuilder;
import org.apache.kafka.streams.StreamsConfig;
import org.apache.kafka.streams.Topology;
import org.apache.kafka.streams.kstream.Consumed;
import org.apache.kafka.streams.kstream.KStream;
import org.apache.kafka.streams.kstream.Produced;

import io.confluent.kafka.serializers.AbstractKafkaAvroSerDeConfig;
import io.confluent.kafka.serializers.KafkaAvroDeserializerConfig;
import io.confluent.kafka.streams.serdes.avro.SpecificAvroSerde;
import messages.CCAuthorization;

public class FraudDetector {  
    static final String SOURCE_TOPIC = "cc-authorizations";
    static final String SINK_TOPIC = "potential-fraud";
    static final String SCHEMA_REGISTRY_URL = "http://schema-registry:8081";

    public static void main(String[] args) {
        doStream();
    }

    private static void doStream(){
        System.out.println("*** Starting Fraud Detector Kafka Streams application ***");
        Properties settings = new Properties();
        settings.put(StreamsConfig.APPLICATION_ID_CONFIG, "fraud-detector");
        settings.put(StreamsConfig.BOOTSTRAP_SERVERS_CONFIG, "kafka:9092");
        settings.put(ConsumerConfig.AUTO_OFFSET_RESET_CONFIG, "earliest");

        Topology topology = getTopology();
        KafkaStreams streams = new KafkaStreams(topology, settings);

        Runtime.getRuntime().addShutdownHook(new Thread(() -> {
            System.out.println("<<< Stopping Fraud Detector Kafka Streams application");
            streams.close();
        }));

        System.out.println("Starting to stream...");
        streams.start();
    }

    private static Topology getTopology(){
        final Serde<String> stringSerde = Serdes.String();
        final Serde<CCAuthorization> valueSerde = new SpecificAvroSerde<CCAuthorization>();
        final Map<String, String> serdeConfig =
            Collections.singletonMap(AbstractKafkaAvroSerDeConfig.SCHEMA_REGISTRY_URL_CONFIG, 
                                     SCHEMA_REGISTRY_URL);
        valueSerde.configure(serdeConfig, false);

        StreamsBuilder builder = new StreamsBuilder();
        KStream<String, CCAuthorization> authorizations = builder
            .stream(SOURCE_TOPIC, Consumed.with(stringSerde, valueSerde));
        authorizations
            .filter((key,value) -> value.getStatus().equals("FAIL"))
            .to(SINK_TOPIC, Produced.with(stringSerde, valueSerde));
        return builder.build();
    }
}