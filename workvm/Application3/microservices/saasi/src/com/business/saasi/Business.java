package com.business.saasi;

import java.io.IOException;
import java.io.PrintWriter;

import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import com.rabbitmq.client.ConnectionFactory;
import com.rabbitmq.client.Connection;
import com.rabbitmq.client.Channel;
/**
 * Servlet implementation class Business
 */
@WebServlet("/Business")
public class Business extends HttpServlet {
	private static final long serialVersionUID = 1L;
       
    /**
     * @see HttpServlet#HttpServlet()
     */
    public Business() {
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
		String io = request.getParameter("io");
		String cpu = request.getParameter("cpu");
		String memory = request.getParameter("memory");
		String timestart = request.getParameter("timestart");
		String timetorun = request.getParameter("timetorun");
		String timeout = request.getParameter("timeout");
		String order = io + " " + cpu + " " + memory + " " + timetorun + " " + timeout + " " + timestart;
		System.out.println("111");
		try{			
			doBusiness(order);
			PrintWriter out = null;  
			out.println("ok");
		} catch(Exception e) {
			System.out.println("send message error");
		}
		//response.sendRedirect("business.jsp");
		
	}

	/**
	 * @see HttpServlet#doPost(HttpServletRequest request, HttpServletResponse response)
	 */
	protected void doPost(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
		// TODO Auto-generated method stub
		doGet(request, response);
	}
	
	private void doBusiness(String order) throws Exception {
		ConnectionFactory factory = new ConnectionFactory();
		factory.setHost("rabbitmq");
		Connection connection = factory.newConnection();
		Channel channel = connection.createChannel();
		channel.exchangeDeclare("mono", "direct");
		channel.basicPublish("mono", "business", null, order.getBytes("UTF-8"));
		
		channel.close();
		connection.close();
	}
}
