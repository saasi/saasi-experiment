package com.business.saasi;

import javax.servlet.ServletException;
import javax.servlet.annotation.WebServlet;
import javax.servlet.http.HttpServlet;
import javax.servlet.http.HttpServletRequest;
import javax.servlet.http.HttpServletResponse;
import java.io.IOException;
import java.io.PrintWriter;
import io.prometheus.client.Counter;
import io.prometheus.client.Gauge;

/**
 * Created by aaa on 2017/12/29.
 */
@WebServlet("/metrics")
public class Metrics extends HttpServlet {
    private String message;
    static final Counter bms_business_violation_total = Counter.build()
            .name("bms_business_violation_total").help("bms_business_violation_total.").register();
    static final Gauge bms_active_transactions = Gauge.build()
            .name("bms_active_transactions").help("bms_active_transactions.").register();
    @Override
    public void init() throws ServletException {
        message = "Hello, this message is from metrics!";
    }

    @Override
    protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException {
        //设置响应内容类型
        resp.setContentType("text/html");

        //设置逻辑实现
        PrintWriter out = resp.getWriter();
        out.println("<h1>" + message + "</h1>");
        out.println("<p>" + "Number of business violations: " + bms_business_violation_total.get() + "</p>");
        out.println("<p>" + "Number of currently running requests: " + bms_active_transactions.get() + "</p>");
    }

    @Override
    public void destroy() {
        super.destroy();
    }
}