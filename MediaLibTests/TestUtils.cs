using FlaneerMediaLib.VideoDataTypes;

namespace MediaLibTests;

public static class TestUtils
{
    public static bool IsValidH264Frame(byte[] frame){
        if (CheckH264FrameHasStartCode(frame))
            return false;
        if (!CheckH264FrameHasHeader(frame))
            return false;
        if (!CheckH264FrameHasPps(frame))
            return false;
        if (!CheckH264FrameHasSps(frame))
            return false;
        if (!CheckH264FrameHasNal(frame))
            return false;
        return true;
    }

    private static bool CheckH264FrameHasStartCode(byte[] frame) => frame[TransmissionVideoFrame.HeaderSize + 0] != 0 ||
                                                                    frame[TransmissionVideoFrame.HeaderSize + 1] != 0 ||
                                                                    frame[TransmissionVideoFrame.HeaderSize + 2] != 0 ||
                                                                    frame[TransmissionVideoFrame.HeaderSize + 3] != 1;

    private static bool CheckH264FrameHasHeader(byte[] frame) => frame[TransmissionVideoFrame.HeaderSize + 4] == 0x67;

    private static bool CheckH264FrameHasPps(byte[] frame) => frame[TransmissionVideoFrame.HeaderSize + 5] == 0x68;

    private static bool CheckH264FrameHasSps(byte[] frame) => frame[TransmissionVideoFrame.HeaderSize + 6] == 0x6e;

    private static bool CheckH264FrameHasNal(byte[] frame) => frame[TransmissionVideoFrame.HeaderSize + 7] == 0x65;
}
