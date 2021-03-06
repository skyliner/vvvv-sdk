﻿using System;
using System.Runtime.InteropServices;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Hosting.Pins
{
	/// <summary>
	/// Base class of 2d spreads.
	/// </summary>
	[ComVisible(false)]
	abstract class BinSpread<T> : Spread<ISpread<T>>
	{
		internal class BinSpreadStream : BufferedIOStream<ISpread<T>>
		{
			protected override void BufferIncreased(ISpread<T>[] oldBuffer, ISpread<T>[] newBuffer)
			{
				Array.Copy(oldBuffer, newBuffer, oldBuffer.Length);
				if (oldBuffer.Length > 0)
				{
					for (int i = oldBuffer.Length; i < newBuffer.Length; i++)
					{
						var spread = oldBuffer[i & (oldBuffer.Length - 1)];
						if (spread != null)
						    newBuffer[i] = (Spread<T>) spread.Clone();
						else
							newBuffer[i] = new Spread<T>(0);
					}
				}
				else
				{
					for (int i = oldBuffer.Length; i < newBuffer.Length; i++)
					{
						newBuffer[i] = new Spread<T>(0);
					}
				}
			}
		}
		
		protected readonly IIOFactory FIOFactory;
		
		public BinSpread(IIOFactory ioFactory, IOAttribute attribute, BinSpreadStream stream)
			: base(stream)
		{
			FIOFactory = ioFactory;
		}
	}
}
