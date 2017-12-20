package com.globalMonitor;

import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.MalformedURLException;
import java.net.URL;

import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;



import java.sql.*;
import com.dao.BusinessDao;

/**
 * Servlet implementation class businessViolation
 */
//@WebServlet("/businessViolation")
public class businessViolation extends HttpServlet {
	private static final long serialVersionUID = 1L;
       
    /**
     * @see HttpServlet#HttpServlet()
     */
    public businessViolation() {
        super();
        // TODO Auto-generated constructor stub
    }

	/**
	 * @see HttpServlet#doGet(HttpServletRequest request, HttpServletResponse response)
	 */
	protected void doGet(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
		// TODO Auto-generated method stub
		//response.getWriter().append("Served at: ").append(request.getContextPath());
		request.setCharacterEncoding("utf-8");
		String bmsGuid = request.getParameter("bms");
		String remoteAddr = request.getRemoteAddr();
		System.out.println(remoteAddr);
		int value = BusinessDao.readBmsViolation(bmsGuid); 
		if (value == 0){//record business violation count;
			if (BusinessDao.readBmsCount(remoteAddr) == 0)
				BusinessDao.writeBmsCount(remoteAddr,1);
			BusinessDao.writeBmsViolation(bmsGuid, 1);
		}
		BusinessDao.updateBmsViolation(bmsGuid, value + 1);	
		System.out.println(value);
		
		if (value + 1 > 5){
			
			Timestamp ts = BusinessDao.readbmsScaleTime(bmsGuid);
			int bmsCount = BusinessDao.readBmsCount(remoteAddr);
			if (ts == null){
				java.util.Date date = new java.util.Date();
				Timestamp timeStamp = new Timestamp(date.getTime());
				BusinessDao.writeBmsScale(remoteAddr, bmsGuid, timeStamp);
				
				sendScaleOrder(remoteAddr,bmsGuid, bmsCount + 1);
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
					sendScaleOrder(remoteAddr,bmsGuid, bmsCount + 1);
					BusinessDao.updateBmsViolation(bmsGuid, 1);
					BusinessDao.writeBmsCount(remoteAddr,bmsCount + 1); 
				}
			}
				
		}
	}
	private void sendScaleOrder(String vmAddress, String bmsGuid, int bmsCount){
		try{
	        URL url = new URL("http://" + vmAddress + ":5002/ScaleOut?bmsGuid=" + bmsGuid + "&bmsCount=" + bmsCount);
	        HttpURLConnection conn = (HttpURLConnection) url.openConnection();
	        InputStream is = conn.getInputStream();
		} catch (MalformedURLException e) {
            e.printStackTrace();
        } catch (IOException e) {
            e.printStackTrace();
        }

	}

	/**
	 * @see HttpServlet#doPost(HttpServletRequest request, HttpServletResponse response)
	 */
	protected void doPost(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
		// TODO Auto-generated method stub
		doGet(request, response);
	}

}
