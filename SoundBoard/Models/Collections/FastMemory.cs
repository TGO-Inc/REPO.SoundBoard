using System.Buffers;

namespace SoundBoard.Models.Collections;

public class FastMemory
{
    private readonly ArrayPool<float> _pool = ArrayPool<float>.Shared;
    
    /// <summary>
    /// Free an audio buffer.
    /// </summary>
    /// <param name="audioBuf"><see cref="float"/>[]</param>
    public void FreeArray(float[] audioBuf) => _pool.Return(audioBuf, true);
    
    /// <summary>
    /// Create a new audio buffer.
    /// </summary>
    /// <param name="length"><see cref="int"/></param>
    /// <returns></returns>
    public float[] NewArray(int length) => _pool.Rent(length);
    
    /// <summary>
    /// Create a new audio buffer.
    /// </summary>
    /// <param name="length"><see cref="long"/></param>
    /// <returns></returns>
    public float[] NewArray(long length) => _pool.Rent((int)length);
}