package sparkexample;

import com.rabbitmq.client.*;

import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.SocketPermission;
import java.net.URL;
import java.text.SimpleDateFormat;
import java.util.Date;
import java.util.UUID;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;
import java.util.concurrent.TimeUnit;
import java.util.concurrent.TimeoutException;

/**
 * Created by Dylan on 2017/7/20.
 */
public class businessFunction {
    private static String id = UUID.randomUUID().toString();
    private static final String _rabbitmqHost = "rabbitmq";

    public static void main(String[] args) throws Exception {
        System.out.println("================== Waiting for RabbitMQ to start");

        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost(_rabbitmqHost);
        boolean connected = false;
        while (!connected) {
            try {
                Connection connection = factory.newConnection();
                System.out.println("================== Connected");
                connected = true;
            } catch (Exception e) {
                System.out.println("================== Not connected, retrying in 500ms");
            }
            Thread.sleep(500);
        }

        businessFunction bf1 = new businessFunction();
        bf1.run();
        businessFunction bf2 = new businessFunction();
        bf2.run();
        businessFunction bf3 = new businessFunction();
        bf3.run();
    }

    public businessFunction(){
        
    }

    public  void run() throws IOException, TimeoutException {
        System.out.println("waiting call");
        ExecutorService executor = Executors.newFixedThreadPool(20);
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost(_rabbitmqHost);
        Connection connection = factory.newConnection();
        Channel channel = connection.createChannel();
        channel.exchangeDeclare("mono", "direct");
        String queueName = "business_queue";
        channel.queueDeclare(queueName,true,false,false,null);
        channel.basicQos(0, 20, false);
        channel.queueBind(queueName, "mono", "business");
        Consumer consumer = new DefaultConsumer(channel) {
            @Override
            public void handleDelivery(String consumerTag, Envelope envelope,
                                       AMQP.BasicProperties properties, byte[] body) throws IOException {
                String message = new String(body, "UTF-8");
                SimpleDateFormat df = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
                System.out.println("call API " + df.format(new Date()));
                try {
                    callApi(message);
                    BusinessWorker bw = new BusinessWorker(message, channel, envelope);
                    executor.execute(bw);
                } catch (TimeoutException e) {
                    e.printStackTrace();
                }

                try {
                    TimeUnit.MILLISECONDS.sleep(10);
                } catch (InterruptedException e) {
                }
            }
        };
        channel.basicConsume(queueName, false, consumer);
    }

    public void callApi(String message) throws IOException, TimeoutException {
        ConnectionFactory factory = new ConnectionFactory();
        factory.setHost(_rabbitmqHost);
        Connection connection = factory.newConnection();
        Channel channel = connection.createChannel();
        channel.exchangeDeclare("call", "direct");
        String[] order = message.split(" ");
        if (order[0].equals("1")) {
            channel.basicPublish("call", "io", null, message.getBytes("UTF-8"));
        }
        if (order[1].equals("1")) {
            channel.basicPublish("call", "cpu", null, message.getBytes("UTF-8"));
        }
        if (order[2].equals("1")) {
            channel.basicPublish("call", "memory", null, message.getBytes("UTF-8"));
        }
    }

    class BusinessWorker implements Runnable{
        private String message;
        private Channel channel;
        private Envelope envelope;
        public BusinessWorker(String message, Channel channel,Envelope envelope) {
            this.channel = channel;
            this.message = message;
            this.envelope = envelope;

        }
        @Override
        public void run() {
            long start =  Long.parseLong(message.split(" ")[5]) * 1000;
            int timetorun = Integer.parseInt(message.split(" ")[3]);
            int timeout = Integer.parseInt(message.split(" ")[4]);
            long out = (Long.parseLong(message.split(" ")[5]) + timeout) * 1000;

            Date startTime = new Date(start);
            long recieve = new Date().getTime();
        //    System.out.println(startTime.toString());

            try {
                Thread.sleep(timetorun * 1000);
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
            long finish = new Date().getTime();
            if (finish > out) {
                System.out.println(id);
                try {
                    System.out.println(start /1000 +" " + recieve / 1000 +" " + finish / 1000 + " " + out / 1000);
                    URL url = new URL("http://10.137.0.81:5002/BusinessTimeout?bmsguid=" + id);
                    HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                    InputStream is = conn.getInputStream();

                } catch (MalformedURLException e) {
                    e.printStackTrace();
                } catch (IOException e) {
                    e.printStackTrace();
                }
            }
            try {
                channel.basicAck(envelope.getDeliveryTag(), false);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }
}

