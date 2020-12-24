Shader "Custom/FinalOutline"
{
	Properties//Variables
	{
		_MainTex("Main Texture (RBG)", 2D) = "white" {}//Allows for a texture property.
		_Color("Color", Color) = (1,1,1,1)//Allows for a color property.

		_OutlineWidth("Outline Width", Range(1.0,10.0)) = 1.1

		_BlurRadius("Blur Radius", Range(0.0,20.0)) = 1
		_Intensity("Blur Intensity", Range(0.0,1.0)) = 0.01

		_DistortColor("Distort Color", Color) = (1,1,1,1)
		_BumpAmt("Distortion", Range(0,128)) = 10
		_DistortTex("Distort Texture (RGB)", 2D) = "white" {}
		_BumpMap("Normal Map", 2D) = "bump" {}
	}

	SubShader
	{
		Tags
		{
			"Queue" = "Transparent"
		}

		GrabPass{}//Grabs everything behind the object.
		UsePass "Custom/OutlineDistort/OUTLINEDISTORT"//Distorts everything behind the object plus the outline width.
		GrabPass{}//Grabs the distorted pass.
		UsePass "Custom/OutlineBlur/OUTLINEHORIZONTALBLUR"//Blurs the distortion on the horizontal axis plus a bit more than the outline width.
		GrabPass{}//Grabs the horizontally blurred distortion.
		UsePass "Custom/OutlineBlur/OUTLINEVERTICALBLUR"//Blurs the horizontally blurred distortion on the vertical axis.
		
		UsePass "Custom/Outline/OBJECT"//Renders the object.
	}
}
