using FlaneerMediaLib.VideoDataTypes;

namespace MediaLibTests;

public static class TestUtils
{
    public static bool IsValid(byte[] frame)
    {
        const int headerOffset = TransmissionVideoFrame.HeaderSize;

        // Check the H.264 header
        if (frame[headerOffset] != 0x00 || 
            frame[headerOffset + 1] != 0x00 || 
            frame[headerOffset + 2] != 0x00 || 
            frame[headerOffset + 3] != 0x01)
        {
            return false;
        }

        // Check the NAL unit type
        int nalUnitType = frame[headerOffset + 4] & 0x1F;

        // Check the SPS and PPS parameter sets
        bool spsFound = false;
        bool ppsFound = false;
        for (int i = headerOffset; i < frame.Length - 4; i++)
        {
            if (frame[i] == 0x00 && frame[i + 1] == 0x00 && frame[i + 2] == 0x00 && frame[i + 3] == 0x01)
            {
                // Check the NAL unit type
                nalUnitType = frame[i + 4] & 0x1F;
                if (nalUnitType == 7)
                {
                    spsFound = true;
                }
                else if (nalUnitType == 8)
                {
                    ppsFound = true;
                }
            }
        }
        if (!spsFound || !ppsFound)
        {
            return false;
        }

        // Check the VUI parameters
        for (int i = headerOffset + 5; i < frame.Length - 4; i++)
        {
            if (frame[i] == 0x00 && frame[i + 1] == 0x00 && frame[i + 2] == 0x00 && frame[i + 3] == 0x01)
            {
                // Check the NAL unit type
                nalUnitType = frame[i + 4] & 0x1F;
                if (nalUnitType == 7)
                {
                    // SPS NAL unit found
                    // Check if the VUI parameters are present
                    if ((frame[i + 6] & 0x80) != 0x80)
                    {
                        return false;
                    }
                }
            }
        }

        // All checks passed, H.264 header is valid
        return true;
    }
}
