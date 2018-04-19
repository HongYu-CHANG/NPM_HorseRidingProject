﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.IO.Ports;
using System.Threading;

public class horseControl : MonoBehaviour {

	public GameObject playerPosition;
	public GameObject playerController;
	private float disOfPlayerAndHorse;
	private Animator _animator;
	private int pressure;

	// Arduino connection
	private CommunicateWithArduino Uno = new CommunicateWithArduino();
	
	void Start ()
	{
		//new Thread(Uno.connectToArdunio).Start
		_animator = this.GetComponent<Animator>();
		disOfPlayerAndHorse = 4f;
	}

	// Update is called once per frame
	void Update()
	{

		if (Input.GetKeyDown(KeyCode.H))
		{
			Debug.Log("Press H");

			transform.position = playerPosition.transform.position;
			transform.forward = playerPosition.transform.forward;
			transform.position -= transform.up.normalized * 2f;
			transform.position -= transform.forward.normalized * 1.3f;
			Debug.Log((playerController.transform.position.y - transform.position.y));
			if((playerController.transform.position.y - transform.position.y) > disOfPlayerAndHorse)
				disOfPlayerAndHorse = playerController.transform.position.y - transform.position.y;
		}
		transform.eulerAngles= new Vector3(0, transform.eulerAngles.y, transform.eulerAngles.z);//保持馬的水平
		playerController.transform.position = new Vector3(playerController.transform.position.x, transform.position.y + disOfPlayerAndHorse, playerController.transform.position.z);
		
		pressure = 0;//Uno.ReceiveData();
		if (pressure < 100) // 走路
		{
			_animator.SetInteger("horseSpeed", 1);
		}
		else if (pressure < 200) //小跑步
		{
			_animator.SetInteger("horseSpeed", 2);
		}
		else //瘋狂跑
		{ 
			_animator.SetInteger("horseSpeed", 3);
		}
	}

	class CommunicateWithArduino
	{
		public bool connected = true;
		public bool mac = false;
		public string choice = "cu.usbmodem1421";
		private SerialPort arduinoController;

		public void connectToArdunio()
		{

			if (connected)
			{
				string portChoice = "COM6";
				if (mac)
				{
					int p = (int)Environment.OSVersion.Platform;
					// Are we on Unix?
					if (p == 4 || p == 128 || p == 6)
					{
						List<string> serial_ports = new List<string>();
						string[] ttys = Directory.GetFiles("/dev/", "cu.*");
						foreach (string dev in ttys)
						{
							if (dev.StartsWith("/dev/tty."))
							{
								serial_ports.Add(dev);
								Debug.Log(String.Format(dev));
							}
						}
					}
					portChoice = "/dev/" + choice;
				}
				arduinoController = new SerialPort(portChoice, 9600, Parity.None, 8, StopBits.One);
				arduinoController.Handshake = Handshake.None;
				arduinoController.RtsEnable = true;
				arduinoController.Open();
				Debug.LogWarning(arduinoController);
			}

		}
		public void SendData(object obj)
		{
			string data = obj as string;
			Debug.Log(data);
			if (connected)
			{
				if (arduinoController != null)
				{
					arduinoController.Write(data);
					arduinoController.Write("\n");
				}
				else
				{
					Debug.Log(arduinoController);
					Debug.Log("nullport");
				}
			}
			else
			{
				Debug.Log("not connected");
			}
			Thread.Sleep(500);
		}

		public int ReceiveData()
		{
			return int.Parse(arduinoController.ReadLine());
		}
	}

}
