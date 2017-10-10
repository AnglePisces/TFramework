using System;
namespace Tools
{
	public class TArrayBuffer<T>
	{
		private const int DFLT_SIZE = 524288;
		private int space = 0;
		private int available = 0;
		private int capacity = 524288;
		private int wr_nxt = 0;
		private int rd_nxt = 0;
		private T[] dataBuf;
		public int Space
		{
			get
			{
				return this.space;
			}
		}
		public int Available
		{
			get
			{
				return this.available;
			}
		}
		public int Capacity
		{
			get
			{
				return this.capacity;
			}
			set
			{
				bool flag = value < this.available || value == 0;
				if (!flag)
				{
					bool flag2 = value == this.capacity;
					if (!flag2)
					{
						T[] array = new T[value];
						bool flag3 = this.available > 0;
						if (flag3)
						{
							this.available = this.ReadData(array, 0, array.Length, true);
						}
						this.dataBuf = array;
						this.capacity = value;
						this.space = this.capacity - this.available;
						this.rd_nxt = 0;
						this.wr_nxt = ((this.space == 0) ? 0 : this.available);
					}
				}
			}
		}
		public TArrayBuffer() : this(524288)
		{
		}
		public TArrayBuffer(int capacity) : this(new T[capacity])
		{
		}
		public TArrayBuffer(T[] buf) : this(buf, 0, 0)
		{
		}
		public TArrayBuffer(T[] buf, int offset, int size)
		{
			this.dataBuf = buf;
			this.capacity = buf.Length;
			this.available = size;
			this.space = this.capacity - this.available;
			this.rd_nxt = offset;
			this.wr_nxt = offset + size;
		}
		public void Clear()
		{
			this.available = 0;
			this.space = this.capacity;
			this.rd_nxt = 0;
			this.wr_nxt = 0;
		}
		public T PeekOne()
		{
			T[] array = new T[1];
			this.Peek(array);
			return array[0];
		}
		public T ReadOne()
		{
			T[] array = new T[1];
			this.Read(array);
			return array[0];
		}
		public int Peek(T[] buf)
		{
			return this.Peek(buf, 0, buf.Length);
		}
		public T[] Peek(int size)
		{
			bool flag = this.Available < size;
			T[] result;
			if (flag)
			{
				result = null;
			}
			else
			{
				T[] array = new T[size];
				int num = this.Peek(array);
				bool flag2 = num < size;
				if (flag2)
				{
					result = null;
				}
				else
				{
					result = array;
				}
			}
			return result;
		}
		public T[] Read(int size)
		{
			bool flag = this.Available < size;
			T[] result;
			if (flag)
			{
				result = null;
			}
			else
			{
				T[] array = new T[size];
				int num = this.Read(array);
				bool flag2 = num < size;
				if (flag2)
				{
					result = null;
				}
				else
				{
					result = array;
				}
			}
			return result;
		}
		public int Read(T[] buf)
		{
			return this.Read(buf, 0, buf.Length);
		}
		public int Read(T[] buf, int offset, int size)
		{
			int num = this.ReadData(buf, offset, size, true);
			this.space += num;
			this.available -= num;
			return num;
		}
		public int Peek(T[] buf, int offset, int size)
		{
			return this.ReadData(buf, offset, size, false);
		}
		private int ReadData(T[] buf, int offset, int size, bool RemoveData)
		{
			int arg_13_0 = (this.available >= size) ? size : this.available;
			int num;
			int num2;
			int result = this.PeekData(buf, offset, size, out num, out num2);
			if (RemoveData)
			{
				this.rd_nxt = num;
				this.wr_nxt = num2;
			}
			return result;
		}
		private int PeekData(T[] buf, int offset, int size, out int readPoint, out int writePoint)
		{
			readPoint = this.rd_nxt;
			writePoint = this.wr_nxt;
			int num = (this.available >= size) ? size : this.available;
			int num2 = this.capacity - readPoint;
			bool flag = readPoint < writePoint || num2 >= num;
			if (flag)
			{
				Array.Copy(this.dataBuf, readPoint, buf, offset, num);
				readPoint += num;
			}
			else
			{
				Array.Copy(this.dataBuf, readPoint, buf, offset, num2);
				readPoint = num - num2;
				Array.Copy(this.dataBuf, 0, buf, offset + num2, readPoint);
			}
			return num;
		}
		public void Write(byte[] buf)
		{
			this.Write(buf, 0, buf.Length);
		}
		public void Write(byte[] buf, int offset, int size)
		{
			int i = size;
			int num = offset;
			while (i > 0)
			{
				int num2 = (this.space >= i) ? i : this.space;
				int num3 = this.capacity - this.wr_nxt;
				bool flag = this.wr_nxt < this.rd_nxt || num3 >= num2;
				if (flag)
				{
					Array.Copy(buf, num, this.dataBuf, this.wr_nxt, num2);
					this.wr_nxt += num2;
					bool flag2 = this.wr_nxt == this.capacity;
					if (flag2)
					{
						this.wr_nxt = 0;
					}
				}
				else
				{
					Array.Copy(buf, num, this.dataBuf, this.wr_nxt, num3);
					this.wr_nxt = num2 - num3;
					Array.Copy(buf, num + num3, this.dataBuf, 0, this.wr_nxt);
				}
				this.space -= num2;
				this.available += num2;
				num += num2;
				i -= num2;
			}
		}
		public void DestroySelf()
		{
			this.Clear();
			this.dataBuf = null;
		}
	}
}
