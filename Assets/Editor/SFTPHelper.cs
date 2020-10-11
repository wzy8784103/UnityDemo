using Tamir.SharpSsh.jsch;
using System;
using System.Collections;
using UnityEngine;

public class SFTPHelper
{
    private Session session;
    private Channel channel;
    private ChannelSftp sftp;

    public SFTPHelper(string ip, string user, string password, int port = 22)
    {
        JSch jsch = new JSch();
        session = jsch.getSession(user, ip, port);
        session.setPassword(password);
        Hashtable foo = new Hashtable();
        //不加会默认key登陆
        foo.Add("StrictHostKeyChecking", "no");
        session.setConfig(foo);
    }

    //SFTP连接状态        
    public bool IsConnect
    {
        get
        {
            return session.isConnected();
        }
    }

    //连接SFTP        
    public bool Connect()
    {
        try
        {
            if (!IsConnect)
            {
                session.connect();
                channel = session.openChannel("sftp");
                channel.connect();
                sftp = (ChannelSftp)channel;
            }
            return true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.Log(ex.Message);
            return false;
        }
    }

    //断开SFTP        
    public void Disconnect()
    {
        if (IsConnect)
        {
            channel.disconnect();
            session.disconnect();
        }
    }

    //SFTP存放文件        
    public bool Put(string localPath, string remotePath)
    {
        try
        {
            Tamir.SharpSsh.java.String src = new Tamir.SharpSsh.java.String(localPath);
            Tamir.SharpSsh.java.String dst = new Tamir.SharpSsh.java.String(remotePath);
            sftp.put(src, dst);
            return true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(ex.Message);
            return false;
        }
    }

    //SFTP获取文件        
    public bool Get(string remotePath, string localPath)
    {
        try
        {
            string dirPath = localPath.Substring(0, localPath.LastIndexOf('/'));
            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }
            //if (!System.IO.File.Exists(localPath))
            //{
            //    System.IO.File.Create(localPath).Dispose();
            //}
            Tamir.SharpSsh.java.String src = new Tamir.SharpSsh.java.String(remotePath);
            Tamir.SharpSsh.java.String dst = new Tamir.SharpSsh.java.String(localPath);
            sftp.get(src, dst);
            return true;
        }
        catch (Exception ex)
        {
            UnityEngine.Debug.LogError(ex.Message);
            return false;
        }
    }
    //删除SFTP文件
    public bool Delete(string remoteFile)
    {
        try
        {
            sftp.rm(remoteFile);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static bool UpLoad(SFTPHelper sftp, string localPath, string remotePath)
	{
        try
        {
            bool result = sftp.Connect();
            result = sftp.Put(localPath, remotePath);
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError("ex===" + ex.Message);
        }
        finally
        {
            sftp.Disconnect();
        }
        return false;
    }
    public static bool DownLoad(SFTPHelper sftp, string localPath, string remotePath)
    {
        try
        {
            bool result = sftp.Connect();
            result = sftp.Get(remotePath, localPath);
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError("ex===" + ex.Message);
        }
        finally
        {
            sftp.Disconnect();
        }
        return false;
    }
}