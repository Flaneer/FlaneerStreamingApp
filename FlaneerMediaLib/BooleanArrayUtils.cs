namespace FlaneerMediaLib;

/// <summary>
/// Converts a byte to an array of bools and vice versa 
/// </summary>
internal static class BooleanArrayUtils
{
    public static byte ConvertBoolArrayToByte(params bool[] source)
    {
        var sourceLen = source.Length;
        var byteSize = 8; //8 bits in a byte
        if (sourceLen > byteSize)
            throw new Exception("More than 8 bools provided");
        if (sourceLen < byteSize)
        {
            var sourceCopy = new bool[sourceLen];
            source.CopyTo(sourceCopy, 0);
            source = new bool[byteSize];
            for (int i = 0; i < byteSize; i++)
            {
                if (i < sourceCopy.Length)
                    source[i] = sourceCopy[i];
                else
                    source[i] = false;
            }
        }
        
        int index = 0;
        byte result = 0;
        // Loop through the array
        foreach (bool b in source)
        {
            // if the element is 'true' set the bit at that position
            if (b)
                result |= (byte)(1 << (7 - index));

            index++;
        }

        return result;
    }

    public static byte SetSingleBit(byte current, bool value, int idx)
    {
        var currentBools = ConvertByteToBoolArray(current);
        currentBools[idx] = value;
        return ConvertBoolArrayToByte(currentBools);
    }
    
    public static bool[] ConvertByteToBoolArray(byte b)
    {
        // prepare the return result
        bool[] result = new bool[8];

        // check each bit in the byte. if 1 set to true, if 0 set to false
        for (int i = 0; i < 8; i++)
            result[i] = (b & (1 << i)) != 0;

        // reverse the array
        Array.Reverse(result);

        return result;
    }
}
