import React, { useEffect, useState } from 'react';
import { Settings, Save, RefreshCw } from 'lucide-react';
import { adminGetSettings, adminUpdateSetting } from '../services/api';
import type { GameSetting } from '../types';
import { styles } from '../styles/shared';

const SettingsPage: React.FC = () => {
  const [settings, setSettings] = useState<GameSetting[]>([]);
  const [loading, setLoading]   = useState(true);
  const [saving, setSaving]     = useState(false);
  const [saved, setSaved]       = useState(false);
  const [changed, setChanged]   = useState<Record<string, string>>({});

  const fetch = () => {
    setLoading(true);
    adminGetSettings().then(r => {
      setSettings(r.data || []);
      setChanged({});
    }).finally(() => setLoading(false));
  };

  useEffect(() => { fetch(); }, []);

  const handleChange = (key: string, value: string) => {
    setChanged(prev => ({...prev, [key]: value}));
    setSettings(prev => prev.map(s => s.setting_key === key ? {...s, setting_value: value} : s));
  };

  const handleSave = async () => {
    const keys = Object.keys(changed);
    if (keys.length === 0) { setSaved(true); setTimeout(() => setSaved(false), 1500); return; }
    setSaving(true);
    try {
      await Promise.all(keys.map(key => adminUpdateSetting(key, changed[key])));
      setChanged({});
      setSaved(true);
      setTimeout(() => setSaved(false), 2000);
    } catch { alert('Gagal menyimpan beberapa setting.'); }
    finally { setSaving(false); }
  };

  const GROUPS = [
    {
      label: '🎮 Gameplay',
      keys: ['location_question_count','trivia_question_count','battle_question_count','xp_per_level','character_change_level'],
    },
    {
      label: '⚔ Battle',
      keys: ['battle_wait_timeout','battle_reward_xp','battle_reward_stars'],
    },
    {
      label: '📊 Pemain Baru',
      keys: ['initial_xp','initial_stars'],
    },
  ];

  return (
    <div className="dashboard-container">
      <div className="welcome-header" style={{ minHeight: 80 }}>
        <div className="welcome-profile">
          <div className="stat-icon-wrap"><Settings size={24}/></div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.4rem' }}>Pengaturan Game</h2>
            <p style={{ margin: 0, fontSize: '0.8rem', opacity: 0.8 }}>Konfigurasi disimpan ke database — berlaku langsung</p>
          </div>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={fetch} style={styles.btnSecondary}><RefreshCw size={14}/> Reload</button>
          <button onClick={handleSave} disabled={saving} style={{
            ...styles.btnPrimary,
            background: saved ? 'rgba(76,175,80,0.3)' : undefined,
            borderColor: saved ? '#4CAF50' : undefined,
          }}>
            <Save size={14}/> {saving ? 'Menyimpan...' : saved ? 'Tersimpan!' : `Simpan${Object.keys(changed).length > 0 ? ` (${Object.keys(changed).length})` : ''}`}
          </button>
        </div>
      </div>

      {loading ? (
        <div className="wood-panel" style={{ textAlign: 'center', padding: 40, opacity: 0.5 }}>Memuat setting...</div>
      ) : GROUPS.map(group => {
        const groupSettings = settings.filter(s => group.keys.includes(s.setting_key));
        if (groupSettings.length === 0) return null;
        return (
          <div key={group.label} className="wood-panel">
            <div className="panel-head">
              <h3>{group.label}</h3>
              {Object.keys(changed).some(k => group.keys.includes(k)) && (
                <span style={{ fontSize: '0.72rem', color: '#FF9800' }}>● Ada perubahan belum disimpan</span>
              )}
            </div>
            <div style={{ display: 'flex', flexDirection: 'column', gap: 10 }}>
              {groupSettings.map(s => (
                <div key={s.setting_key} style={{ display: 'flex', alignItems: 'center', gap: 20, padding: '12px 16px', background: changed[s.setting_key] !== undefined ? 'rgba(212,175,55,0.06)' : 'rgba(0,0,0,0.15)', borderRadius: 8, border: changed[s.setting_key] !== undefined ? '1px solid rgba(212,175,55,0.3)' : '1px solid rgba(255,255,255,0.04)', transition: '0.2s' }}>
                  <div style={{ flex: 1 }}>
                    <div style={{ fontWeight: 'bold', fontSize: '0.85rem', marginBottom: 2 }}>{s.description}</div>
                    <div style={{ fontSize: '0.7rem', opacity: 0.45, fontFamily: 'monospace' }}>{s.setting_key}</div>
                  </div>
                  <input
                    type="number"
                    value={s.setting_value}
                    onChange={e => handleChange(s.setting_key, e.target.value)}
                    style={{ width: 110, padding: '8px 12px', background: 'rgba(0,0,0,0.4)', border: '1px solid #5C3B26', borderRadius: 6, color: '#D4AF37', fontSize: '0.95rem', textAlign: 'center', outline: 'none', fontWeight: 'bold' }}
                  />
                </div>
              ))}
            </div>
          </div>
        );
      })}

      {/* Setting yang tidak masuk grup manapun */}
      {(() => {
        const allGroupKeys = GROUPS.flatMap(g => g.keys);
        const ungrouped = settings.filter(s => !allGroupKeys.includes(s.setting_key));
        if (ungrouped.length === 0) return null;
        return (
          <div className="wood-panel">
            <div className="panel-head"><h3>⚙ Lainnya</h3></div>
            {ungrouped.map(s => (
              <div key={s.setting_key} style={{ display: 'flex', alignItems: 'center', gap: 20, padding: '12px 16px', background: 'rgba(0,0,0,0.15)', borderRadius: 8, marginBottom: 8 }}>
                <div style={{ flex: 1 }}>
                  <div style={{ fontWeight: 'bold', fontSize: '0.85rem' }}>{s.description || s.setting_key}</div>
                  <div style={{ fontSize: '0.7rem', opacity: 0.4, fontFamily: 'monospace' }}>{s.setting_key}</div>
                </div>
                <input
                  type="text"
                  value={s.setting_value}
                  onChange={e => handleChange(s.setting_key, e.target.value)}
                  style={{ width: 140, padding: '8px 12px', background: 'rgba(0,0,0,0.4)', border: '1px solid #5C3B26', borderRadius: 6, color: '#D4AF37', fontSize: '0.9rem', textAlign: 'center', outline: 'none' }}
                />
              </div>
            ))}
          </div>
        );
      })()}
    </div>
  );
};

export default SettingsPage;