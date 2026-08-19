import React, { useEffect, useState, useRef } from 'react';
import { Music, Upload, Trash2, RefreshCw, Play, Pause, Volume2 } from 'lucide-react';
import { adminGetMusic, adminUploadMusic, adminDeleteMusic } from '../services/api';
import type { MusicFile } from '../types';
import { styles } from '../styles/shared';

const MusicPage: React.FC = () => {
  const [files, setFiles]     = useState<MusicFile[]>([]);
  const [loading, setLoading] = useState(true);
  const [uploading, setUploading] = useState(false);
  const [playing, setPlaying]   = useState<string | null>(null);
  const [msg, setMsg]           = useState('');
  const audioRef = useRef<HTMLAudioElement | null>(null);
  const fileRef  = useRef<HTMLInputElement>(null);

  const fetch = () => {
    setLoading(true);
    adminGetMusic().then(r => setFiles(r.data || [])).finally(() => setLoading(false));
  };

  useEffect(() => { fetch(); }, []);

  const handleUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0]; if (!file) return;
    setUploading(true); setMsg('');
    try {
      await adminUploadMusic(file);
      setMsg('✓ File berhasil diupload!');
      fetch();
    } catch { setMsg('✗ Gagal upload. Pastikan format mp3/ogg/wav.'); }
    finally { setUploading(false); e.target.value = ''; }
  };

  const handleDelete = async (filename: string) => {
    if (!confirm(`Hapus file "${filename}"?`)) return;
    try {
      await adminDeleteMusic(filename);
      if (playing === filename) stopAudio();
      fetch();
    } catch { setMsg('Gagal menghapus file.'); }
  };

  const playAudio = (file: MusicFile) => {
    stopAudio();
    
    // Gunakan URL dari response API langsung
    const audioUrl = `http://localhost:8080${file.url}`;
    
    try {
      const audio = new Audio(audioUrl);
      
      audio.onerror = () => {
        setMsg('✗ Gagal memutar audio. File mungkin tidak ditemukan di server.');
      };
      
      audio.oncanplaythrough = () => {
        audio.play()
          .then(() => {
            audioRef.current = audio;
            setPlaying(file.filename);
          })
          .catch(() => {
            setMsg('✗ Browser memblokir autoplay. Coba klik lagi.');
          });
      };
      
      audio.load();
      audio.onended = () => setPlaying(null);
      
    } catch (err) {
      setMsg('✗ Gagal memutar audio.');
    }
  };
  const stopAudio = () => {
    audioRef.current?.pause();
    audioRef.current = null;
    setPlaying(null);
  };

  const formatSize = (bytes: number) => {
    if (bytes < 1024) return bytes + ' B';
    if (bytes < 1024 * 1024) return (bytes / 1024).toFixed(1) + ' KB';
    return (bytes / 1024 / 1024).toFixed(2) + ' MB';
  };

  return (
    <div className="dashboard-container">
      <div className="welcome-header" style={{ minHeight: 80 }}>
        <div className="welcome-profile">
          <div className="stat-icon-wrap"><Music size={24}/></div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.4rem' }}>Kelola Musik & Audio</h2>
            <p style={{ margin: 0, fontSize: '0.8rem', opacity: 0.8 }}>Backsound dan efek suara untuk APK game</p>
          </div>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={fetch} style={styles.btnSecondary}><RefreshCw size={14}/> Refresh</button>
          <button onClick={() => fileRef.current?.click()} style={styles.btnPrimary} disabled={uploading}>
            <Upload size={14}/> {uploading ? 'Mengupload...' : 'Upload Audio'}
          </button>
          <input ref={fileRef} type="file" accept=".mp3,.ogg,.wav" style={{ display: 'none' }} onChange={handleUpload}/>
        </div>
      </div>

      {msg && (
        <div style={{ padding: '12px 20px', borderRadius: 8, background: msg.startsWith('✓') ? 'rgba(76,175,80,0.15)' : 'rgba(244,67,54,0.15)', border: `1px solid ${msg.startsWith('✓') ? '#4CAF50' : '#F44336'}`, fontSize: '0.85rem' }}>
          {msg}
        </div>
      )}

      {/* Info penggunaan */}
      <div className="wood-panel" style={{ borderLeft: '4px solid #D4AF37' }}>
        <div style={{ fontSize: '0.82rem', lineHeight: 1.8, opacity: 0.8 }}>
          <strong style={{ color: '#D4AF37' }}>Panduan Penggunaan Audio di Unity:</strong><br/>
          1. Upload file audio di sini (format mp3/ogg/wav)<br/>
          2. Catat nama file yang muncul di daftar<br/>
          3. Di Unity, buka <code style={{ background: 'rgba(0,0,0,0.3)', padding: '1px 6px', borderRadius: 3 }}>SoundManager.cs</code><br/>
          4. Pada method <code style={{ background: 'rgba(0,0,0,0.3)', padding: '1px 6px', borderRadius: 3 }}>PlayBGM("bgm_main")</code>, sesuaikan nama dengan file yang diupload<br/>
          5. File akan di-fetch dari server saat game berjalan
        </div>
      </div>

      {/* Daftar File */}
      <div className="wood-panel">
        <div className="panel-head">
          <h3>File Audio ({files.length})</h3>
          {playing && (
            <button onClick={stopAudio} style={{...styles.btnSecondary, color: '#F44336', borderColor: '#F44336'}}>
              <Pause size={13}/> Stop
            </button>
          )}
        </div>

        {loading ? (
          <div style={{ textAlign: 'center', padding: 40, opacity: 0.5 }}>Memuat...</div>
        ) : files.length === 0 ? (
          <div style={{ textAlign: 'center', padding: 60, opacity: 0.4 }}>
            <Music size={48} style={{ margin: '0 auto 12px', display: 'block' }}/>
            <p>Belum ada file audio.</p>
            <p style={{ fontSize: '0.8rem' }}>Upload file mp3/ogg/wav untuk backsound game.</p>
          </div>
        ) : (
          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: 12 }}>
            {files.map(file => (
              <div key={file.filename} style={{ background: 'rgba(0,0,0,0.2)', borderRadius: 10, padding: 14, border: playing === file.filename ? '1px solid #D4AF37' : '1px solid rgba(255,255,255,0.05)', transition: '0.2s' }}>
                {/* Wave animation jika playing */}
                <div style={{ display: 'flex', alignItems: 'center', gap: 10, marginBottom: 10 }}>
                  <div style={{ width: 40, height: 40, borderRadius: '50%', background: playing === file.filename ? 'rgba(212,175,55,0.2)' : 'rgba(0,0,0,0.3)', display: 'flex', alignItems: 'center', justifyContent: 'center', border: playing === file.filename ? '1px solid #D4AF37' : 'none' }}>
                    <Volume2 size={18} color={playing === file.filename ? '#D4AF37' : '#F5F5DC'}/>
                  </div>
                  <div style={{ flex: 1, overflow: 'hidden' }}>
                    <div style={{ fontWeight: 'bold', fontSize: '0.82rem', whiteSpace: 'nowrap', overflow: 'hidden', textOverflow: 'ellipsis' }}>{file.filename}</div>
                    <div style={{ fontSize: '0.7rem', opacity: 0.5 }}>{file.type.toUpperCase()} • {formatSize(file.size)}</div>
                  </div>
                </div>

                <div style={{ display: 'flex', gap: 8 }}>
                  <button
                    onClick={() => playing === file.filename ? stopAudio() : playAudio(file)}
                    style={{ flex: 1, ...styles.btnPrimary, justifyContent: 'center' }}
                  >
                    {playing === file.filename ? <><Pause size={13}/> Stop</> : <><Play size={13}/> Preview</>}
                  </button>
                  <button onClick={() => handleDelete(file.filename)} style={{ ...styles.btnIcon, color: '#F44336' }}>
                    <Trash2 size={15}/>
                  </button>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default MusicPage;