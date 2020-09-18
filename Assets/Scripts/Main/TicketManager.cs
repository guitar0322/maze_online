using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;
using UnityEngine;
using UnityEngine.UI;
using BackEnd;
using UnityEngine.PlayerLoop;

public class TicketManager : MonoBehaviour
{
    private static int TICKET_TIME = 600;
    private static int FULL_TICKET = 5;
    private float time;
    private bool full;
    public Text ticket;
    public Text ticketTime;
    public DateTime userIndate;
    public DateTime now;
    public TimeSpan timespan;

    // Start is called before the first frame update
    void Start()
    {
        now = DateTime.Now;
        userIndate = Convert.ToDateTime(UserInfo.instance.inDate);
        timespan = now - userIndate;
        time = TICKET_TIME;
        full = false;
        TicketInit();
        Debug.Log("now : " + now.ToString() + " , indate : " + userIndate.ToString() + "compare : " + (int)timespan.TotalSeconds);
    }

    public void TicketInit()
    {
        int curTicket = Convert.ToInt32(UserInfo.instance.ticket);
        int addTicket = (int)timespan.TotalSeconds / TICKET_TIME; //ex 668 / 600 = 1

        UpdateTicket(curTicket, addTicket);
    }

    public void UpdateTicket(int curTicket, int addTicket)
    {
        BackendReturnObject BRO;
        Param param = new Param();
        int totalTicket = curTicket + addTicket;

        if (totalTicket >= FULL_TICKET)
        {
            UserInfo.instance.inDate = now.ToString("yyyy-MM-dd'T'HH:mm:ss");
            ticketTime.gameObject.SetActive(false);
            full = true;
        }
        else
        {
            //게임시작할때 티켓이 4장으로 감소하면 indate를 현재시간으로 업데이트 해야함
            userIndate.AddMinutes((TICKET_TIME * addTicket)/ 60);
            UserInfo.instance.inDate = userIndate.ToString("yyyy-MM-dd'T'HH:mm:ss");
            time -= (int)timespan.TotalSeconds % TICKET_TIME;
        }

        UserInfo.instance.ticket = "" + totalTicket;
        ticket.text = "" + totalTicket;
        param.Add("indate", UserInfo.instance.inDate);
        BRO = Backend.GameInfo.Update("character", UserInfo.instance.DBindate, param);
    }

    private void FixedUpdate()
    {
        if(full == false)
        {
            time -= Time.fixedDeltaTime;
            ticketTime.text = string.Format("{0:D2} : {1:D2}", (int)(time / 60), (int)(time % 60));
            if(time <= 0)
            {
                UpdateTicket(Convert.ToInt32(UserInfo.instance.ticket), 1);

            }
        }
    }
}
