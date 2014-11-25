/*
 *
 *	Adventure Creator
 *	by Chris Burton, 2013-2014
 *	
 *	"ParticleSwitch.cs"
 * 
 *	This can be used, via the Object: Send Message Action,
 *	to turn it's attached particle systems on and off.
 * 
 */

using UnityEngine;
using System.Collections;

public class ParticleSwitch : MonoBehaviour
{
	
	public bool enableOnStart = false;
	
	
	private void Awake ()
	{
		Switch (enableOnStart);
	}
	
	
	public void TurnOn ()
	{
		Switch (true);
	}
	
	
	public void TurnOff ()
	{
		Switch (false);
	}


	public void Interact ()
	{
		if (this.particleSystem)
		{
			this.particleSystem.Emit (this.particleSystem.maxParticles);
		}
	}
	
	
	private void Switch (bool turnOn)
	{
		if (this.particleSystem)
		{
			if (turnOn)
			{
				this.particleSystem.Play ();
			}
			else
			{
				this.particleSystem.Stop ();
			}
		}
	}
	
}
