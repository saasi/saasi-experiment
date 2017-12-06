package com.dao;
import java.sql.*;

public class BusinessDao {
	
	public static Connection connectDb(){
		final String driver = "com.mysql.jdbc.Driver";
		final String host = System.getenv("MYSQL_HOST");
		String url = "jdbc:mysql://"+host+"/db?useUnicode=true&amp;characterEncoding=utf-8&useSSL=false";
	    String username = System.getenv("MYSQL_USERNAME");//"root";
		String password = System.getenv("MYSQL_PASSWORD");//"MyNewPass4!";
		String dbName = System.getenv("MYSQL_DB");

	    Connection conn = null;
    	try{
    		Class.forName(driver);
			//conn = (Connection) DriverManager.getConnection("jdbc:mysql://127.0.0.1:3306/saasi-experiment", username, password);
    		conn = (Connection) DriverManager.getConnection("jdbc:mysql://"+host+"/"+dbName, username, password);
    	}catch (ClassNotFoundException e){
    		e.printStackTrace();
    	}catch (SQLException e){
    		e.printStackTrace();
    	}
    	if(conn!=null){
    		System.out.println("连接成功！");
    	}else{
    		System.out.println("连接失败！");
    	}
    	return conn;
	}
	
	public static void writeBmsViolation(String bmsGuid, int value){
		Connection conn = connectDb();
		Statement stmt = null;
		ResultSet rs = null;

			try{
				stmt = conn.createStatement();
				String sql = "insert into bmsviolationcount (bmsGuid, violationCount) values ('" + bmsGuid + "', " + value + ")";
				stmt.executeUpdate(sql);
				System.out.println("insert sql 执行成功");
			} catch (SQLException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}finally{
				try{
			         if(stmt!=null)
			            stmt.close();
			    }catch(SQLException se){
			    	se.printStackTrace();
			    }try{
			    if(conn!=null)
			            conn.close();
			    }catch(SQLException se){
			         se.printStackTrace();
			    }	    
			}
	}
	
	public static void updateBmsViolation(String bmsGuid, int value){
		Connection conn = connectDb();
		Statement stmt = null;
		ResultSet rs = null;
		try{
			stmt = conn.createStatement();
			String sql = "update bmsviolationcount set violationCount=" + value + " where bmsGuid='" + bmsGuid + "'";
			stmt.executeUpdate(sql);
			System.out.println("update sql 执行成功");
		} catch (SQLException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}finally{
			try{
		         if(stmt!=null)
		            stmt.close();
		    }catch(SQLException se){
		    	se.printStackTrace();
		    }try{
		    if(conn!=null)
		            conn.close();
		    }catch(SQLException se){
		         se.printStackTrace();
		    }	    
		}
	}
	public static int readBmsCount(String vmAddress){
		int value = 0;
		Connection conn = connectDb();
		Statement stmt = null;
		ResultSet rs = null;
		try{
			stmt = conn.createStatement();
			String sql = "select bmsCount FROM vmCount where vmAddress='"+vmAddress+"'";
			rs = stmt.executeQuery(sql);
			System.out.println("查询成功");
			if (rs == null)
				value = 0;
			else {
				while(rs.next()){
					value = rs.getInt("bmsCount");
				}
					
			}
		} catch (SQLException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}finally{
			try{
		         if(stmt!=null)
		            stmt.close();
		    }catch(SQLException se){
		    }try{
		    if(conn!=null)
		            conn.close();
		    }catch(SQLException se){
		         se.printStackTrace();
		    }
		}
		return value;
	}
	public static void writeBmsCount(String vmAddress, int bmsCount){
		Connection conn = connectDb();
		Statement stmt = null;
		ResultSet rs = null;
		if (bmsCount == 1){
			try{
				stmt = conn.createStatement();
				String sql = "insert into vmcount (vmAddress, bmsCount) values ('" + vmAddress + "', " + bmsCount + ")";
				stmt.executeUpdate(sql);
				System.out.println("insert sql 执行成功");
			} catch (SQLException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}finally{
				try{
			         if(stmt!=null)
			            stmt.close();
			    }catch(SQLException se){
			    	se.printStackTrace();
			    }try{
			    if(conn!=null)
			            conn.close();
			    }catch(SQLException se){
			         se.printStackTrace();
			    }	    
			}
		}else {
			try{
				stmt = conn.createStatement();
				String sql = "update vmCount set bmsCount=" + bmsCount + " where vmAddress='" + vmAddress + "'";
				stmt.executeUpdate(sql);
				System.out.println("update sql 执行成功");
			} catch (SQLException e) {
				// TODO Auto-generated catch block
				e.printStackTrace();
			}finally{
				try{
			         if(stmt!=null)
			            stmt.close();
			    }catch(SQLException se){
			    	se.printStackTrace();
			    }try{
			    if(conn!=null)
			            conn.close();
			    }catch(SQLException se){
			         se.printStackTrace();
			    }	    
			}
		}
	}
	

	public static void updateBmsScale(String bmsGuid, Timestamp ts){
		Connection conn = connectDb();
		Statement stmt = null;
		ResultSet rs = null;
		try{
			stmt = conn.createStatement();
			String sql = "update businessscaleout set lastScaleTime='" + ts + "' where bmsGuid='" + bmsGuid + "'";
			System.out.println(sql);
			stmt.executeUpdate(sql);
			System.out.println("update sql 执行成功");
		} catch (SQLException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}finally{
			try{
		         if(stmt!=null)
		            stmt.close();
		    }catch(SQLException se){
		    	se.printStackTrace();
		    }try{
		    if(conn!=null)
		            conn.close();
		    }catch(SQLException se){
		         se.printStackTrace();
		    }	    
		}
	}
	
	public static void writeBmsScale(String vmAddress, String bmsGuid, Timestamp ts){
		Connection conn = connectDb();
		Statement stmt = null;
		ResultSet rs = null;
		try{
			stmt = conn.createStatement();
			String sql = "insert into businessscaleout (bmsGuid, vmAddress, lastScaleTime) values ('" + bmsGuid + "', '" + vmAddress + "', '" + ts + "')";
			System.out.println(sql);
			stmt.executeUpdate(sql);
			System.out.println("insert sql 执行成功");
		} catch (SQLException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}finally{
			try{
		         if(stmt!=null)
		            stmt.close();
		    }catch(SQLException se){
		    	se.printStackTrace();
		    }try{
		    if(conn!=null)
		            conn.close();
		    }catch(SQLException se){
		         se.printStackTrace();
		    }	    
		}
	}
	
	public static Timestamp readbmsScaleTime(String bmsGuid){
		Connection conn = connectDb();
		Statement stmt = null;
		ResultSet rs = null;
		Timestamp ts = null;
		try{
			stmt = conn.createStatement();
			String sql = "select lastScaleTime FROM businessscaleout where bmsGuid='" + bmsGuid + "'";
			rs = stmt.executeQuery(sql);
			System.out.println("查询成功");
			if (rs == null)
				ts = null;
			else {
				while(rs.next()){
					ts = rs.getTimestamp("lastScaleTime");
				}
					
			}
		} catch (SQLException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}finally{
			try{
		         if(stmt!=null)
		            stmt.close();
		    }catch(SQLException se){
		    }try{
		    if(conn!=null)
		            conn.close();
		    }catch(SQLException se){
		         se.printStackTrace();
		    }
		}
		return ts;
	}
	public static int readBmsViolation(String bmsGuid){
		int value = 0;
		Connection conn = connectDb();
		Statement stmt = null;
		ResultSet rs = null;
		try{
			stmt = conn.createStatement();
			String sql = "select violationCount FROM bmsviolationcount where bmsGuid='"+bmsGuid+"'";
			rs = stmt.executeQuery(sql);
			System.out.println("查询成功");
			if (rs == null)
				value = 0;
			else {
				while(rs.next()){
					value = rs.getInt("violationCount");
				}
					
			}
		} catch (SQLException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}finally{
			try{
		         if(stmt!=null)
		            stmt.close();
		    }catch(SQLException se){
		    }try{
		    if(conn!=null)
		            conn.close();
		    }catch(SQLException se){
		         se.printStackTrace();
		    }
		}
		return value;
	}
	
	public static void resetData(){
		Connection conn = connectDb();
		Statement stmt = null;

		try{
			stmt = conn.createStatement();
			String sql = "delete from bmsviolationcount";
			stmt.executeUpdate(sql);
			sql = "delete from businessscaleout";
			stmt.executeUpdate(sql);
			sql = "delete from vmcount";
			stmt.executeUpdate(sql);
		} catch (SQLException e) {
			// TODO Auto-generated catch block
			e.printStackTrace();
		}finally{
			try{
		         if(stmt!=null)
		            stmt.close();
		    }catch(SQLException se){
		    }try{
		    if(conn!=null)
		            conn.close();
		    }catch(SQLException se){
		         se.printStackTrace();
		    }
		}
	}
}
