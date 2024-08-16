using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SC_inv_mover : MonoBehaviour {

	public Transform Communtron1;
	public Transform Communtron2;
	public Transform Communtron4;
	public SC_control SC_control;
	public SC_backpack SC_backpack;

	public bool active = false;
	float closed;
	float opened;

	public float step_size = 17f;
	public float A;
	public bool backwards;
	public bool updown;
	public bool tab;
	public bool inv;

	void Update()
	{
		//Inventory extender
		if (inv && !SC_control.pause)
		{
			if (!active && Input.GetKeyDown(KeyCode.E) && !SC_control.SC_chat.typing && Communtron1.position.z == 0f)
				active = true;

			else if (active && (Input.GetKeyDown(KeyCode.E) || Input.GetKeyDown(KeyCode.Escape) || Communtron1.position.z != 0f))
			{
				active = false;
				SC_control.blockEscapeThisFrame = true;
			}
		}

		//Tab extender
		if (tab && (int)Communtron4.position.y == 100)
		{
			if (Input.GetKey(KeyCode.Tab)) active = true;
			else active = false;
		}
	}

	void LateUpdate()
	{
		float step = step_size * Time.deltaTime * 50f;

		if (!updown)
		{
			if (active)
			{
				if (transform.localPosition.x > opened)
				{
					transform.localPosition -= new Vector3(step, 0f, 0f);
					if (transform.localPosition.x < opened)
					{
						transform.localPosition = new Vector3(opened, transform.localPosition.y, transform.localPosition.z);
					}
				}
				else if (transform.localPosition.x < opened)
				{
					transform.localPosition += new Vector3(step, 0f, 0f);
					if (transform.localPosition.x > opened)
					{
						transform.localPosition = new Vector3(opened, transform.localPosition.y, transform.localPosition.z);
					}
				}
			}
			else
			{
				if (transform.localPosition.x > closed)
				{
					transform.localPosition -= new Vector3(step, 0f, 0f);
					if (transform.localPosition.x < closed)
					{
						transform.localPosition = new Vector3(closed, transform.localPosition.y, transform.localPosition.z);
					}
				}
				else if (transform.localPosition.x < closed)
				{
					transform.localPosition += new Vector3(step, 0f, 0f);
					if (transform.localPosition.x > closed)
					{
						transform.localPosition = new Vector3(closed, transform.localPosition.y, transform.localPosition.z);
					}
				}
			}
		}

		if (updown)
		{
			if (active)
			{
				if (transform.localPosition.y > opened)
				{
					transform.localPosition -= new Vector3(0f, step, 0f);
					if (transform.localPosition.y < opened)
					{
						transform.localPosition = new Vector3(transform.localPosition.x, opened, transform.localPosition.z);
					}
				}
				else if (transform.localPosition.y < opened)
				{
					transform.localPosition += new Vector3(0f, step, 0f);
					if (transform.localPosition.y > opened)
					{
						transform.localPosition = new Vector3(transform.localPosition.x, opened, transform.localPosition.z);
					}
				}
			}
			else
			{
				if (transform.localPosition.y > closed)
				{
					transform.localPosition -= new Vector3(0f, step, 0f);
					if (transform.localPosition.y < closed)
					{
						transform.localPosition = new Vector3(transform.localPosition.x, closed, transform.localPosition.z);
					}
				}
				else if (transform.localPosition.y < closed)
				{
					transform.localPosition += new Vector3(0f, step, 0f);
					if (transform.localPosition.y > closed)
					{
						transform.localPosition = new Vector3(transform.localPosition.x, closed, transform.localPosition.z);
					}
				}
			}
		}
	}

	void Start()
	{
		if (!updown) closed = transform.localPosition.x;
		else closed = transform.localPosition.y;
		if (!backwards) opened = closed + (A * step_size);
		else opened = closed - (A * step_size);
	}
}
