package com.dao;

import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;
import java.sql.Timestamp;

public class Test {

	public static void main(String[] args) {
		// TODO Auto-generated method stub
		//System.out.println(BusinessDao.readBmsViolation("123456"));
	/*	String remoteAddr = "111.112";
		String bmsGuid = "1234567";
		int value = BusinessDao.readBmsViolation(bmsGuid); 
		if (value == 0){
			BusinessDao.writeBmsCount(remoteAddr,1);
			BusinessDao.writeBmsViolation(bmsGuid, 1);
		}
		BusinessDao.updateBmsViolation(bmsGuid, value + 1);
		System.out.println(value);
		
		if (value + 1 > 5){
			
			Timestamp ts = BusinessDao.readbmsScaleTime(bmsGuid);
			int bmsCount = BusinessDao.readBmsCount(remoteAddr);
			//System.out.println(ts);
			if (ts == null){
				java.util.Date date = new java.util.Date();
				Timestamp timeStamp = new Timestamp(date.getTime());
				BusinessDao.writeBmsScale(remoteAddr, bmsGuid, timeStamp);
				
				//sendScaleOrder(remoteAddr,bmsGuid, bmsCount + 1);
				BusinessDao.writeBmsCount(remoteAddr,bmsCount + 1);
				BusinessDao.updateBmsViolation(bmsGuid, 1);
			}
			else {
				long lastTime = ts.getTime();
				java.util.Date date = new java.util.Date();
				long currentTime = date.getTime();
				Timestamp timeStamp = new Timestamp(date.getTime());
				if (currentTime - lastTime > 30 * 1000){
					BusinessDao.updateBmsScale(bmsGuid, timeStamp);
					//sendScaleOrder(remoteAddr,bmsGuid, bmsCount + 1);
					BusinessDao.updateBmsViolation(bmsGuid, 1);
				}
			}
				
		}*/
		try{
	        URL url = new URL("http://l10.137.0.81:8080/globalMonitor/resetData");
	        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
	        InputStream is = conn.getInputStream();
		} catch (MalformedURLException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }

/*		try{
	        URL url = new URL("http://l10.137.0.81:8080/globalMonitor/businessViolation?bms=c");
	        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
	        InputStream is = conn.getInputStream();
		} catch (MalformedURLException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }

		try{
	        URL url = new URL("http://localhost:8080/globalMonitor/businessViolation?bms=c");
	        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
	        InputStream is = conn.getInputStream();
		} catch (MalformedURLException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }*/

	}

}
