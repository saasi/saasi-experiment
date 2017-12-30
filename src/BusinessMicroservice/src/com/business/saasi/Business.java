package com.business.saasi;

import java.io.IOException;
import java.io.PrintWriter;
import java.util.Date;
import java.util.UUID;

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
	public static String id = UUID.randomUUID().toString();
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
		Metrics.bms_active_transactions.inc();
		request.setCharacterEncoding("utf-8");
		// Get URL parameters
		String io = request.getParameter("io");
		String cpu = request.getParameter("cpu");
		String memory = request.getParameter("memory");
		String timestart = request.getParameter("timestart");
		String timetorun = request.getParameter("timetorun");
		String timeout = request.getParameter("timeout");       // We will compare this with the actual processing time,
		                                                        // to determine if there is a business violation

		PrintWriter out = response.getWriter();

		// Start a new BusinessWorker to process this request
		BusinessWorker bw = new BusinessWorker(io.equals("1"), cpu.equals("1"), memory.equals("1"),
				Long.parseLong(timestart), Integer.parseInt(timetorun), Integer.parseInt(timeout));
		/*BusinessWorker bw = new BusinessWorker(true, true, true,
				new Date().getTime()/1000, 3, 3);*/
		try {
			bw.CallMicroservices();
		} catch (Exception ex) {
			out.println("Exception:" +ex);
		}
		
		
		out.println("OK.");
		Metrics.bms_active_transactions.dec();
	}

	/**
	 * @see HttpServlet#doPost(HttpServletRequest request, HttpServletResponse response)
	 */
	protected void doPost(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
		// TODO Auto-generated method stub
		doGet(request, response);
	}
	
}
