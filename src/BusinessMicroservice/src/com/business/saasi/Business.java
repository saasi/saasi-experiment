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
import io.prometheus.client.Counter;
import io.prometheus.client.Gauge;

/**
 * Servlet implementation class Business
 */
@WebServlet("/Business")
public class Business extends HttpServlet {
	static final Counter bms_business_violation_total = Counter.build()
		.name("bms_business_violation_total")
        .labelNames("io", "cpu", "memory", "timetorun")
		.help("bms_business_violation_total.")
		.register();
    static final Gauge bms_active_transactions = Gauge.build()
		.name("bms_active_transactions")
		.labelNames("io", "cpu", "memory", "timetorun")
		.help("bms_active_transactions.")
		.register();

	private static final long serialVersionUID = 1L;

    /**
     * @see HttpServlet#HttpServlet()
     */
    public Business() {
        super();

    }

    private int tryParseInt(String s) {
        int result;
        try {
            result = Integer.parseInt(s);
        } catch (NumberFormatException | NullPointerException e) {
            return 0;
        }
        return result;
    }

    private long tryParseLong(String s) {
        long result;
        try {
            result = Long.parseLong(s);
        } catch (NumberFormatException | NullPointerException e) {
            return 0;
        }
        return result;
    }
	/**
	 * @see HttpServlet#doGet(HttpServletRequest request, HttpServletResponse response)
	 */
	protected void doGet(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {

        String transcationID = UUID.randomUUID().toString();
        System.out.println("Transcation "+ transcationID + ": started");


		request.setCharacterEncoding("utf-8");
		// Get URL parameters
		String s_io = request.getParameter("io");
		int io = tryParseInt(s_io);
		String s_cpu = request.getParameter("cpu");
		int cpu = tryParseInt(s_cpu);
		String s_memory = request.getParameter("memory");
		int memory = tryParseInt(s_memory);
		String s_timestart = request.getParameter("timestart");
		long timestart = tryParseLong(s_timestart);
		String s_timetorun = request.getParameter("timetorun");
		int timetorun = tryParseInt(s_timetorun);
		String s_timeout = request.getParameter("timeout");       // We will compare this with the actual processing time,
		int timeout = tryParseInt(s_timeout);                           // to determine if there is a business violation
        bms_active_transactions
            .labels(new Integer(io).toString(),
                    new Integer(cpu).toString(),
                    new Integer(memory).toString(),
                    new Integer(timetorun).toString())
            .inc();


		// Start a new BusinessWorker to process this request
		BusinessWorker bw = new BusinessWorker(io == 1, cpu == 1, memory == 1,
				timestart, timetorun, timeout);
		BusinessWorkerResult result = null;
		try {
			result = bw.CallMicroservices();
		} catch (Exception ex) {
			//out.println("Exception:" +ex);
		}
        System.out.println("Transcation "+ transcationID + ": finished");
		if (result != null) {
		    if (result.violated) {
                System.out.println("Transcation "+ transcationID + ": violated");
                Business.bms_business_violation_total
                    .labels(new Integer(io).toString(),
                            new Integer(cpu).toString(),
                            new Integer(memory).toString(),
                            new Integer(timetorun).toString())
                    .inc();
            }
        }

        PrintWriter out = response.getWriter();
        bms_active_transactions
            .labels(new Integer(io).toString(),
                    new Integer(cpu).toString(),
                    new Integer(memory).toString(),
                    new Integer(timetorun).toString())
            .dec();
		out.println("OK.");
	}

	/**
	 * @see HttpServlet#doPost(HttpServletRequest request, HttpServletResponse response)
	 */
	protected void doPost(HttpServletRequest request, HttpServletResponse response) throws ServletException, IOException {
		// TODO Auto-generated method stub
		doGet(request, response);
	}
	
}
