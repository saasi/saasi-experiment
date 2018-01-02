package com.business.saasi;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.io.BufferedReader;
import java.net.UnknownHostException;
import java.util.concurrent.CountDownLatch;

public class ApiCaller implements Runnable {
    private final CountDownLatch DoneSignal;
    private final MicroserviceEnum Type;
    private final int SecondsToRun; 
    private static final String URL_IO = "http://io/api/io?time=";
    private static final String URL_CPU = "http://cpu/api/cpu?time=";
    private static final String URL_MEMORY = "http://memory/api/memory?time=";

    ApiCaller (MicroserviceEnum type, int secondsToRun, CountDownLatch doneSignal) {
        this.Type = type;
        this.SecondsToRun = secondsToRun;
        this.DoneSignal = doneSignal;
    }

    @Override
    public void run() {
        try {
            MakeHTTPCall();
            DoneSignal.countDown();
        } catch (Exception ex) {
            // do nothing
        } // return;
    }

    private void MakeHTTPCall() {
        try {
            URL url;
            switch (this.Type) {
                case IoMicroservice:
                    url = new URL(URL_IO + this.SecondsToRun);
                    break;
                case CpuMicroservice:
                    url = new URL(URL_CPU + this.SecondsToRun);
                    break;
                case MemoryMicroservice:
                    url = new URL(URL_MEMORY + this.SecondsToRun);
                    break;
                default:
                    return;
            }
            System.out.println(url);
            HttpURLConnection conn = (HttpURLConnection) url.openConnection();
            InputStream is = conn.getInputStream();
            try {
                BufferedReader in = new BufferedReader(
                                        new InputStreamReader(
                                        is));
                String inputLine;
                while ((inputLine = in.readLine()) != null) System.out.println(inputLine);
            } finally {
                is.close();
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}