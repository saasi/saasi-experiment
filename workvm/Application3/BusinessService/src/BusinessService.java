import com.rabbitmq.client.*;

import java.io.IOException;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

/**
 * Created by Dylan on 2017/7/20.
 */
public class BusinessService {

    public static void main(String[] args) throws Exception {
        BusinessService bs1 = new BusinessService();
        bs1.run();
        BusinessService bs2 = new BusinessService();
        bs2.run();
    }


    public  void run() throws IOException, TimeoutException {
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost("localhost");
        Connection connection = factory.newConnection();
        Channel channel = connection.createChannel();
        channel.exchangeDeclare("mono", "direct");
        String queueName = "business_queue";
        channel.queueDeclare(queueName,false,false,false,null);
        channel.basicQos(0, 15, false);
        channel.queueBind(queueName, "mono", "business");
        Consumer consumer = new DefaultConsumer(channel) {
            @Override
            public void handleDelivery(String consumerTag, Envelope envelope,
                                       AMQP.BasicProperties properties, byte[] body) throws IOException {
                String message = new String(body, "UTF-8");
                SimpleDateFormat df = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
                System.out.println(" [x] Received '" + message + "' " + df.format(new Date()));
                try {
                    TimeUnit.MILLISECONDS.sleep(10);
                } catch (InterruptedException e) {
                }
            }
        };
        channel.basicConsume(queueName, true, consumer);
    }

}
