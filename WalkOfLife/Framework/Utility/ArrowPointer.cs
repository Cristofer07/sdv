﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace TheLion.AwesomeProfessions
{
	/// <summary>Vertical arrow indicator to reveal on-screen objects of interest for tracker professions.</summary>
	public class ArrowPointer
	{
		public Texture2D Texture { get; }
		private float _height = -42f, _step = 0f, _maxStep = 3f, _minStep = -3f, _jerk = 1f;

		/// <summary>Construct an instance.</summary>
		/// <param name="texture">Arrow pointer texture.</param>
		public ArrowPointer(Texture2D texture)
		{
			Texture = texture;
		}

		/// <summary>Advance the pointer's vertical offset motion by one step, in a bobbing fashion.</summary>
		public void Bob()
		{
			if (_step == _maxStep || _step == _minStep) _jerk = -_jerk;
			_step += _jerk;
			_height += _step;
		}

		/// <summary>Get the pointer's current vertical offset.</summary>
		public Vector2 GetOffset()
		{
			return new Vector2(0f, _height);
		}
	}
}