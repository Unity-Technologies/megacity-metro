package com.unity.megacity;
import android.os.Bundle;
import android.content.Intent;
import android.content.Context;
import android.media.AudioManager;
import android.content.IntentFilter;
import android.content.BroadcastReceiver;
import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class MegacityMetro extends UnityPlayerActivity 
{
    private VolumeButtonReceiver volumeReceiver;
    
    protected void onCreate(Bundle savedInstanceState) 
    {
        super.onCreate(savedInstanceState);
        volumeReceiver = new VolumeButtonReceiver();
    }
    
    @Override
    protected void onResume() 
    {
        super.onResume();
        IntentFilter filter = new IntentFilter();
        filter.addAction("android.media.VOLUME_CHANGED_ACTION");
        registerReceiver(volumeReceiver, filter);
    }
    
    @Override
    protected void onPause() 
    {
        super.onPause();
        unregisterReceiver(volumeReceiver);
    }
    
    private class VolumeButtonReceiver extends BroadcastReceiver 
    {
        @Override
        public void onReceive(Context context, Intent intent) 
        {
            if ("android.media.VOLUME_CHANGED_ACTION".equals(intent.getAction())) 
            {
                AudioManager audioManager = (AudioManager) context.getSystemService(Context.AUDIO_SERVICE);
                int streamType = intent.getIntExtra("android.media.EXTRA_VOLUME_STREAM_TYPE", -1);

                if (streamType == AudioManager.STREAM_VOICE_CALL)
                {
                    int currentVolumeVoice = audioManager.getStreamVolume(AudioManager.STREAM_VOICE_CALL);
                    int minVolumeVoice = audioManager.getStreamMinVolume(AudioManager.STREAM_VOICE_CALL);
                    int maxVolumeVoice = audioManager.getStreamMaxVolume(AudioManager.STREAM_VOICE_CALL);
                    int maxVolume = audioManager.getStreamMaxVolume(AudioManager.STREAM_MUSIC);

                    float normalizedVolumeVoice = (float) currentVolumeVoice - (float) minVolumeVoice / maxVolumeVoice - minVolumeVoice;
                    int volume = (int) (normalizedVolumeVoice * maxVolume);
                    audioManager.setStreamVolume(AudioManager.STREAM_MUSIC, volume, AudioManager.FLAG_SHOW_UI);
                }
                else if(streamType == AudioManager.STREAM_MUSIC)
                {
                    int currentVolume = audioManager.getStreamVolume(AudioManager.STREAM_MUSIC);
                    int maxVolume = audioManager.getStreamMaxVolume(AudioManager.STREAM_MUSIC);
                    float normalizedVolume = (float) currentVolume / maxVolume;
                    UnityPlayer.UnitySendMessage("AudioManager", "OnReceiveDeviceVolume", String.valueOf(normalizedVolume));
                }
            }
        }
    }
}