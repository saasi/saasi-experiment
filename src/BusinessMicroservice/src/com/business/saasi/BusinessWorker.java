package com.business.saasi;

import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.util.Date;
import java.util.concurrent.CountDownLatch;
import java.util.UUID;

public class BusinessWorker {
    private final long StartTimestamp;
    private final int SecondsToRun;
    private final int TimeoutSeconds;
    private final boolean RunIoMicroservice;
    private final boolean RunCpuMicroservice;
    private final boolean RunMemoryMicroservice;

    private static String URL_REPORT_BUSINESS_VIOLATION = 
        "http://"+System.getenv("BV_REPORT_HOST")+"/globalMonitor/businessViolation?bms=";
    
    public BusinessWorker(boolean runIo, boolean runCpu, boolean runMem, long startTimestamp, int secondsToRun, int timeoutSeconds) {
        this.StartTimestamp = startTimestamp;
        this.SecondsToRun = secondsToRun;
        this.TimeoutSeconds = timeoutSeconds;
        this.RunIoMicroservice = runIo;
        this.RunCpuMicroservice = runCpu;
        this.RunMemoryMicroservice = runMem;
    }

    public void CallMicroservices() {

        long StartTimestampMs =  StartTimestamp * 1000;
        long ExpectedFinishTimeMs = (StartTimestamp + TimeoutSeconds) * 1000;

        Date StartTimeDateTime = new Date(StartTimestampMs);
        long ReceivedTimeMs = new Date().getTime();

        // Call API Microservices and wait for them to finish
        try {
            int count = 0;
            if (RunCpuMicroservice) ++count;
            if (RunIoMicroservice) ++count;
            if (RunMemoryMicroservice) ++count;
            CountDownLatch doneSignal = new CountDownLatch(count);
            if (RunIoMicroservice)
                (new Thread(new ApiCaller(MicroserviceEnum.IoMicroservice, SecondsToRun, doneSignal))).start();
            if (RunCpuMicroservice)
                new Thread(new ApiCaller(MicroserviceEnum.CpuMicroservice, SecondsToRun, doneSignal)).start();
            if (RunMemoryMicroservice)
                new Thread(new ApiCaller(MicroserviceEnum.MemoryMicroservice, SecondsToRun, doneSignal)).start();
            doneSignal.await();           // wait for all REST API calls to finish
        } catch (InterruptedException e) {
            e.printStackTrace();
        }

        long FinishedTimeMs = new Date().getTime();
        if (FinishedTimeMs > ExpectedFinishTimeMs) {
            // There is a business violation, so we need to report it
            
            System.out.println(Business.id);
            try {
                // Send an HTTP request to Monitor (deprecated)
                System.out.println(StartTimestamp +" " + ReceivedTimeMs / 1000 +" " + FinishedTimeMs / 1000 + " " + ExpectedFinishTimeMs / 1000);
                URL url = new URL(URL_REPORT_BUSINESS_VIOLATION + Business.id);
                System.out.println(url);
                HttpURLConnection conn = (HttpURLConnection) url.openConnection();
                InputStream is = conn.getInputStream();
            } catch (MalformedURLException e) {
                e.printStackTrace();
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }
}