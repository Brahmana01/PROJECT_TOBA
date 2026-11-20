import React, { useEffect, useRef, useState } from 'react';
import {
  MapPin,
  Plus,
  Edit2,
  Trash2,
  RefreshCw,
  ChevronDown,
  ChevronUp,
  X,
  Check,
  Upload,
} from 'lucide-react';
import {
  adminGetLocations,
  adminCreateLocation,
  adminUpdateLocation,
  adminUploadLocationImage,
  adminGetFacts,
  adminCreateFact,
  adminDeleteFact,
  adminGetLocQuestions,
  adminCreateLocQuestion,
  adminUpdateLocQuestion,
  adminDeleteLocQuestion,
} from '../services/api';
import type {
  Location,
  LocationFact,
  LocationQuestion,
  QuestionOption,
} from '../types';
import { styles } from '../styles/shared';

const SESSION_QUESTION_COUNT = 10;
const LOCATION_MINIMUM_CORRECT = 6;

function getApiError(error: unknown, fallback: string): string {
  const responseMessage = (
    error as { response?: { data?: { message?: string } } }
  )?.response?.data?.message;
  return responseMessage || fallback;
}

async function compressImage(
  file: File,
  maxWidth = 1000,
  quality = 0.82,
): Promise<File> {
  return new Promise((resolve) => {
    const reader = new FileReader();

    reader.onerror = () => resolve(file);
    reader.onload = (event) => {
      const image = new Image();
      image.onerror = () => resolve(file);
      image.onload = () => {
        let { width, height } = image;

        if (width > maxWidth) {
          height = Math.round((height * maxWidth) / width);
          width = maxWidth;
        }

        const canvas = document.createElement('canvas');
        canvas.width = width;
        canvas.height = height;

        const context = canvas.getContext('2d');
        if (!context) {
          resolve(file);
          return;
        }

        context.drawImage(image, 0, 0, width, height);
        canvas.toBlob(
          (blob) => {
            if (!blob) {
              resolve(file);
              return;
            }
            const filename = file.name.replace(/\.[^.]+$/, '') + '.jpg';
            resolve(new File([blob], filename, { type: 'image/jpeg' }));
          },
          'image/jpeg',
          quality,
        );
      };
      image.src = String(event.target?.result || '');
    };

    reader.readAsDataURL(file);
  });
}

// -----------------------------------------------------------------------------
// MODAL LOKASI
// -----------------------------------------------------------------------------
type LocationModalProps = {
  loc: Location | null;
  nextLevelOrder: number;
  onClose: () => void;
  onSave: () => Promise<void> | void;
};

const LocationModal: React.FC<LocationModalProps> = ({
  loc,
  nextLevelOrder,
  onClose,
  onSave,
}) => {
  const isEdit = Boolean(loc);
  const fileRef = useRef<HTMLInputElement>(null);
  const [form, setForm] = useState({
    name: loc?.name ?? '',
    description: loc?.description ?? '',
    image: loc?.image ?? '',
    reward_xp: loc?.reward_xp ?? 100,
    reward_stars: loc?.reward_stars ?? 3,
    level_order: loc?.level_order ?? nextLevelOrder,
    is_active: loc?.is_active ?? true,
  });
  const [uploading, setUploading] = useState(false);
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');

  const handleUpload = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0];
    if (!file) return;

    setUploading(true);
    setMsg('');
    try {
      const compressed = await compressImage(file);
      const response = await adminUploadLocationImage(compressed);
      setForm((current) => ({ ...current, image: response.data.filename }));
      setMsg('Gambar berhasil diunggah.');
    } catch (error) {
      setMsg(getApiError(error, 'Gagal mengunggah gambar.'));
    } finally {
      setUploading(false);
      if (fileRef.current) fileRef.current.value = '';
    }
  };

  const handleSave = async () => {
    if (!form.name.trim() || !form.description.trim()) {
      setMsg('Nama dan deskripsi lokasi wajib diisi.');
      return;
    }
    if (!Number.isInteger(form.level_order) || form.level_order < 1) {
      setMsg('Urutan level harus berupa angka minimal 1.');
      return;
    }
    if (form.reward_xp < 0 || form.reward_stars < 0) {
      setMsg('Reward XP dan bintang tidak boleh negatif.');
      return;
    }

    setSaving(true);
    setMsg('');
    try {
      const payload = {
        name: form.name.trim(),
        description: form.description.trim(),
        image: form.image.trim(),
        reward_xp: form.reward_xp,
        reward_stars: form.reward_stars,
        level_order: form.level_order,
        is_active: form.is_active,
        // Aturan gameplay dikunci: 10 soal per sesi, minimal 6 benar.
        question_count: SESSION_QUESTION_COUNT,
        minimum_correct: LOCATION_MINIMUM_CORRECT,
      };

      if (isEdit && loc) {
        await adminUpdateLocation(loc.id, payload);
      } else {
        await adminCreateLocation(payload);
      }
      await onSave();
    } catch (error) {
      setMsg(getApiError(error, 'Gagal menyimpan lokasi.'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div style={styles.overlay}>
      <div style={styles.modal}>
        <div style={styles.modalHead}>
          <h3 style={{ color: '#D4AF37', margin: 0 }}>
            {isEdit ? 'Edit Lokasi' : 'Tambah Lokasi Baru'}
          </h3>
          <button onClick={onClose} style={styles.closeBtn} aria-label="Tutup">
            <X size={18} />
          </button>
        </div>

        <div style={styles.modalBody}>
          {msg && <div style={styles.msgBox}>{msg}</div>}

          <div style={styles.grid2}>
            <div style={styles.field}>
              <label style={styles.label}>Nama Lokasi *</label>
              <input
                style={styles.input}
                value={form.name}
                onChange={(event) =>
                  setForm((current) => ({ ...current, name: event.target.value }))
                }
                placeholder="Nama lokasi budaya"
              />
            </div>
            <div style={styles.field}>
              <label style={styles.label}>Urutan Level *</label>
              <input
                style={styles.input}
                type="number"
                min={1}
                value={form.level_order}
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    level_order: Number(event.target.value),
                  }))
                }
              />
              <small style={{ opacity: 0.65 }}>
                Lokasi baru otomatis memakai urutan berikutnya.
              </small>
            </div>
          </div>

          <div style={styles.field}>
            <label style={styles.label}>Deskripsi *</label>
            <textarea
              style={{ ...styles.input, height: 85, resize: 'vertical' }}
              value={form.description}
              onChange={(event) =>
                setForm((current) => ({ ...current, description: event.target.value }))
              }
              placeholder="Deskripsi lokasi..."
            />
          </div>

          <div style={styles.field}>
            <label style={styles.label}>Gambar Lokasi</label>
            <div style={{ display: 'flex', gap: 8, alignItems: 'center' }}>
              <input
                style={{ ...styles.input, flex: 1 }}
                value={form.image}
                onChange={(event) =>
                  setForm((current) => ({ ...current, image: event.target.value }))
                }
                placeholder="Nama file gambar atau unggah gambar"
              />
              <button
                type="button"
                onClick={() => fileRef.current?.click()}
                style={styles.btnSecondary}
                disabled={uploading}
              >
                <Upload size={14} /> {uploading ? 'Mengunggah...' : 'Upload'}
              </button>
              <input
                ref={fileRef}
                type="file"
                accept="image/jpeg,image/png,image/webp"
                style={{ display: 'none' }}
                onChange={handleUpload}
              />
            </div>
            {form.image && (
              <small style={{ color: '#82ca9d', marginTop: 4, display: 'block' }}>
                ✓ {form.image}
              </small>
            )}
          </div>

          <div style={styles.grid2}>
            <div style={styles.field}>
              <label style={styles.label}>Reward XP</label>
              <input
                style={styles.input}
                type="number"
                min={0}
                value={form.reward_xp}
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    reward_xp: Number(event.target.value),
                  }))
                }
              />
            </div>
            <div style={styles.field}>
              <label style={styles.label}>Reward Bintang</label>
              <input
                style={styles.input}
                type="number"
                min={0}
                value={form.reward_stars}
                onChange={(event) =>
                  setForm((current) => ({
                    ...current,
                    reward_stars: Number(event.target.value),
                  }))
                }
              />
            </div>
          </div>

          <div style={{
            padding: 12,
            borderRadius: 8,
            background: 'rgba(76,175,80,0.10)',
            border: '1px solid rgba(76,175,80,0.35)',
            fontSize: '0.8rem',
            lineHeight: 1.6,
          }}>
            <strong>Aturan gameplay lokasi</strong>
            <div>{SESSION_QUESTION_COUNT} soal dipilih acak dari bank soal aktif.</div>
            <div>Minimal jawaban benar untuk menyelesaikan lokasi: {LOCATION_MINIMUM_CORRECT}.</div>
            <div>Bank soal tidak dibatasi maksimal 10.</div>
          </div>

          {isEdit && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 12 }}>
              <input
                type="checkbox"
                checked={form.is_active}
                onChange={(event) =>
                  setForm((current) => ({ ...current, is_active: event.target.checked }))
                }
                id="locationIsActive"
              />
              <label htmlFor="locationIsActive" style={styles.label}>
                Lokasi aktif dan dapat diakses sesuai progres pemain
              </label>
            </div>
          )}
        </div>

        <div style={styles.modalFoot}>
          <button onClick={onClose} style={styles.btnSecondary}>Batal</button>
          <button onClick={handleSave} style={styles.btnPrimary} disabled={saving || uploading}>
            <Check size={14} /> {saving ? 'Menyimpan...' : 'Simpan Lokasi'}
          </button>
        </div>
      </div>
    </div>
  );
};

// -----------------------------------------------------------------------------
// MODAL SOAL JELAJAH
// -----------------------------------------------------------------------------
type QuestionModalProps = {
  locId: number;
  question: LocationQuestion | null;
  onClose: () => void;
  onSave: () => Promise<void> | void;
};

const createEmptyOptions = (): QuestionOption[] => [
  { option_text: '', is_correct: true },
  { option_text: '', is_correct: false },
  { option_text: '', is_correct: false },
  { option_text: '', is_correct: false },
];

const LocationQuestionModal: React.FC<QuestionModalProps> = ({
  locId,
  question,
  onClose,
  onSave,
}) => {
  const isEdit = Boolean(question);
  const [q, setQ] = useState(question?.question ?? '');
  const [exp, setExp] = useState(question?.explanation ?? '');
  const [options, setOptions] = useState<QuestionOption[]>(() => {
    if (question?.options?.length === 4) {
      return question.options.map((option) => ({ ...option }));
    }
    return createEmptyOptions();
  });
  const [isActive, setIsActive] = useState(question?.is_active ?? true);
  const [saving, setSaving] = useState(false);
  const [msg, setMsg] = useState('');

  const setCorrectOption = (selectedIndex: number) => {
    setOptions((current) => current.map((option, index) => ({
      ...option,
      is_correct: index === selectedIndex,
    })));
  };

  const handleSave = async () => {
    if (!q.trim() || !exp.trim()) {
      setMsg('Pertanyaan dan penjelasan wajib diisi.');
      return;
    }
    if (options.length !== 4 || options.some((option) => !option.option_text.trim())) {
      setMsg('Isi tepat 4 pilihan jawaban. Tidak boleh ada pilihan kosong.');
      return;
    }
    if (options.filter((option) => option.is_correct).length !== 1) {
      setMsg('Pilih tepat 1 jawaban benar.');
      return;
    }

    setSaving(true);
    setMsg('');
    try {
      const payload = {
        question: q.trim(),
        explanation: exp.trim(),
        is_active: isActive,
        options: options.map((option) => ({
          id: option.id,
          option_text: option.option_text.trim(),
          is_correct: option.is_correct,
        })),
      };

      if (isEdit && question) {
        await adminUpdateLocQuestion(question.id, payload);
      } else {
        await adminCreateLocQuestion(locId, payload);
      }
      await onSave();
    } catch (error) {
      setMsg(getApiError(error, 'Gagal menyimpan soal Jelajah.'));
    } finally {
      setSaving(false);
    }
  };

  return (
    <div style={styles.overlay}>
      <div style={{ ...styles.modal, maxWidth: 620 }}>
        <div style={styles.modalHead}>
          <h3 style={{ color: '#D4AF37', margin: 0 }}>
            {isEdit ? 'Edit Soal Jelajah' : 'Tambah Soal Jelajah'}
          </h3>
          <button onClick={onClose} style={styles.closeBtn} aria-label="Tutup">
            <X size={18} />
          </button>
        </div>

        <div style={styles.modalBody}>
          {msg && <div style={styles.msgBox}>{msg}</div>}
          <div style={styles.field}>
            <label style={styles.label}>Pertanyaan *</label>
            <textarea
              style={{ ...styles.input, height: 75, resize: 'vertical' }}
              value={q}
              onChange={(event) => setQ(event.target.value)}
              placeholder="Tulis pertanyaan..."
            />
          </div>
          <div style={styles.field}>
            <label style={styles.label}>Penjelasan Jawaban *</label>
            <textarea
              style={{ ...styles.input, height: 65, resize: 'vertical' }}
              value={exp}
              onChange={(event) => setExp(event.target.value)}
              placeholder="Penjelasan yang ditampilkan setelah menjawab..."
            />
          </div>

          <label style={styles.label}>Empat pilihan jawaban (pilih satu yang benar)</label>
          {options.map((option, index) => (
            <div key={index} style={{ display: 'flex', gap: 8, alignItems: 'center', marginBottom: 8 }}>
              <span style={{ color: '#D4AF37', fontWeight: 'bold', width: 20 }}>
                {String.fromCharCode(65 + index)}.
              </span>
              <input
                type="radio"
                name="location-correct-option"
                checked={option.is_correct}
                onChange={() => setCorrectOption(index)}
                aria-label={`Jadikan pilihan ${String.fromCharCode(65 + index)} jawaban benar`}
              />
              <input
                style={{ ...styles.input, flex: 1, margin: 0 }}
                value={option.option_text}
                onChange={(event) => setOptions((current) => current.map((item, itemIndex) =>
                  itemIndex === index ? { ...item, option_text: event.target.value } : item,
                ))}
                placeholder={`Pilihan ${String.fromCharCode(65 + index)}`}
              />
              {option.is_correct && (
                <span style={{ color: '#4CAF50', fontSize: 12, whiteSpace: 'nowrap' }}>✓ Benar</span>
              )}
            </div>
          ))}

          {isEdit && (
            <div style={{ display: 'flex', alignItems: 'center', gap: 8, marginTop: 12 }}>
              <input
                id="locationQuestionActive"
                type="checkbox"
                checked={isActive}
                onChange={(event) => setIsActive(event.target.checked)}
              />
              <label htmlFor="locationQuestionActive" style={styles.label}>
                Soal aktif dan dapat dipilih secara acak
              </label>
            </div>
          )}
        </div>

        <div style={styles.modalFoot}>
          <button onClick={onClose} style={styles.btnSecondary}>Batal</button>
          <button onClick={handleSave} style={styles.btnPrimary} disabled={saving}>
            <Check size={14} /> {saving ? 'Menyimpan...' : 'Simpan Soal'}
          </button>
        </div>
      </div>
    </div>
  );
};

// -----------------------------------------------------------------------------
// HALAMAN UTAMA LOKASI
// -----------------------------------------------------------------------------
const LocationsPage: React.FC = () => {
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);
  const [pageError, setPageError] = useState('');
  const [showLocationModal, setShowLocationModal] = useState(false);
  const [editLocation, setEditLocation] = useState<Location | null>(null);
  const [expandedLocationID, setExpandedLocationID] = useState<number | null>(null);
  const [activeLocation, setActiveLocation] = useState<Location | null>(null);

  const [facts, setFacts] = useState<LocationFact[]>([]);
  const [questions, setQuestions] = useState<LocationQuestion[]>([]);
  const [newFact, setNewFact] = useState('');
  const [showQuestionModal, setShowQuestionModal] = useState(false);
  const [editQuestion, setEditQuestion] = useState<LocationQuestion | null>(null);
  const [subLoading, setSubLoading] = useState(false);

  const fetchLocations = async () => {
    setLoading(true);
    setPageError('');
    try {
      const response = await adminGetLocations();
      setLocations(response.data || []);
    } catch (error) {
      setPageError(getApiError(error, 'Gagal mengambil daftar lokasi.'));
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    let mounted = true;
    adminGetLocations()
      .then((response) => {
        if (mounted) setLocations(response.data || []);
      })
      .catch((error: unknown) => {
        if (mounted) setPageError(getApiError(error, 'Gagal mengambil daftar lokasi.'));
      })
      .finally(() => {
        if (mounted) setLoading(false);
      });

    return () => {
      mounted = false;
    };
  }, []);

  const nextLevelOrder = locations.reduce(
    (highest, location) => Math.max(highest, location.level_order || 0),
    0,
  ) + 1;

  const openExpand = async (location: Location) => {
    if (expandedLocationID === location.id) {
      setExpandedLocationID(null);
      setActiveLocation(null);
      setFacts([]);
      setQuestions([]);
      return;
    }

    setExpandedLocationID(location.id);
    setActiveLocation(location);
    setSubLoading(true);
    setPageError('');
    try {
      const [factsResponse, questionsResponse] = await Promise.all([
        adminGetFacts(location.id),
        adminGetLocQuestions(location.id),
      ]);
      setFacts(factsResponse.data || []);
      setQuestions(questionsResponse.data || []);
    } catch (error) {
      setPageError(getApiError(error, 'Gagal mengambil fakta dan bank soal lokasi.'));
      setFacts([]);
      setQuestions([]);
    } finally {
      setSubLoading(false);
    }
  };

  const refreshLocationQuestions = async (locationID: number) => {
    const response = await adminGetLocQuestions(locationID);
    setQuestions(response.data || []);
  };

  const handleAddFact = async (locationID: number) => {
    if (!newFact.trim()) return;
    try {
      await adminCreateFact(locationID, newFact.trim());
      setNewFact('');
      const response = await adminGetFacts(locationID);
      setFacts(response.data || []);
    } catch (error) {
      setPageError(getApiError(error, 'Gagal menambahkan fakta budaya.'));
    }
  };

  const handleDeleteFact = async (factID: number, locationID: number) => {
    if (!window.confirm('Hapus fakta budaya ini?')) return;
    try {
      await adminDeleteFact(factID);
      const response = await adminGetFacts(locationID);
      setFacts(response.data || []);
    } catch (error) {
      setPageError(getApiError(error, 'Gagal menghapus fakta budaya.'));
    }
  };

  const handleDeactivateQuestion = async (questionID: number, locationID: number) => {
    if (!window.confirm('Nonaktifkan soal ini? Soal tetap tersimpan di bank, tetapi tidak dipilih untuk sesi baru.')) {
      return;
    }
    try {
      await adminDeleteLocQuestion(questionID);
      await refreshLocationQuestions(locationID);
    } catch (error) {
      setPageError(getApiError(error, 'Gagal menonaktifkan soal Jelajah.'));
    }
  };

  const activeQuestionCount = questions.filter((question) => question.is_active).length;

  return (
    <div className="dashboard-container">
      <div className="welcome-header" style={{ minHeight: 80 }}>
        <div className="welcome-profile">
          <div className="stat-icon-wrap"><MapPin size={24} /></div>
          <div>
            <h2 style={{ margin: 0, fontSize: '1.4rem' }}>Kelola Lokasi Jelajah</h2>
            <p style={{ margin: 0, fontSize: '0.8rem', opacity: 0.8 }}>
              {locations.length} lokasi • bank soal dan fakta budaya dapat ditambah
            </p>
          </div>
        </div>
        <div style={{ display: 'flex', gap: 10 }}>
          <button onClick={() => void fetchLocations()} style={styles.btnSecondary}>
            <RefreshCw size={14} /> Refresh
          </button>
          <button
            onClick={() => {
              setEditLocation(null);
              setShowLocationModal(true);
            }}
            style={styles.btnPrimary}
          >
            <Plus size={14} /> Tambah Lokasi
          </button>
        </div>
      </div>

      {pageError && <div className="wood-panel" style={{ color: '#ff9b9b', marginBottom: 12 }}>{pageError}</div>}

      {loading ? (
        <div className="wood-panel" style={{ textAlign: 'center', padding: 40, opacity: 0.6 }}>
          Memuat lokasi...
        </div>
      ) : locations.length === 0 ? (
        <div className="wood-panel" style={{ textAlign: 'center', padding: 40, opacity: 0.7 }}>
          Belum ada lokasi. Gunakan tombol Tambah Lokasi untuk membuat lokasi pertama.
        </div>
      ) : locations.map((location) => (
        <div key={location.id} className="wood-panel" style={{ padding: 0, overflow: 'hidden', marginBottom: 12 }}>
          <div style={{ display: 'flex', alignItems: 'center', padding: '16px 20px', gap: 16 }}>
            <div style={{
              width: 40,
              height: 40,
              borderRadius: '50%',
              background: '#D4AF37',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontWeight: 'bold',
              color: '#2C1810',
              flexShrink: 0,
            }}>
              {location.level_order}
            </div>

            <div style={{ flex: 1, minWidth: 0 }}>
              <div style={{ fontWeight: 'bold', fontSize: '1rem', color: '#D4AF37' }}>
                {location.name}
              </div>
              <div style={{ fontSize: '0.75rem', opacity: 0.7, marginTop: 2 }}>
                {location.description?.substring(0, 100)}{location.description?.length > 100 ? '...' : ''}
              </div>
              <div style={{ display: 'flex', gap: 8, marginTop: 6, flexWrap: 'wrap' }}>
                <span className="badge blue">XP +{location.reward_xp}</span>
                <span className="badge green">⭐ +{location.reward_stars}</span>
                <span className="badge" style={{ background: '#5C3B26' }}>
                  🎲 {SESSION_QUESTION_COUNT} soal per sesi
                </span>
                <span className={`badge ${location.is_active ? 'green' : 'orange'}`}>
                  {location.is_active ? 'Aktif' : 'Nonaktif'}
                </span>
                {location.image && (
                  <span className="badge" style={{ background: '#1e88e5' }}>🖼 {location.image}</span>
                )}
              </div>
            </div>

            <div style={{ display: 'flex', gap: 8, flexShrink: 0 }}>
              <button
                onClick={() => void openExpand(location)}
                style={styles.btnIcon}
                title="Kelola fakta dan bank soal"
              >
                {expandedLocationID === location.id ? <ChevronUp size={16} /> : <ChevronDown size={16} />}
              </button>
              <button
                onClick={() => {
                  setEditLocation(location);
                  setShowLocationModal(true);
                }}
                style={styles.btnIcon}
                title="Edit lokasi"
              >
                <Edit2 size={16} />
              </button>
            </div>
          </div>

          {expandedLocationID === location.id && (
            <div style={{
              borderTop: '1px solid rgba(212,175,55,0.2)',
              padding: '16px 20px',
              background: 'rgba(0,0,0,0.2)',
            }}>
              {subLoading ? (
                <p style={{ opacity: 0.6 }}>Memuat fakta dan bank soal...</p>
              ) : (
                <div style={styles.grid2}>
                  <div>
                    <div style={{ fontWeight: 'bold', color: '#D4AF37', marginBottom: 10, fontSize: '0.9rem' }}>
                      💡 Fakta Budaya ({facts.length})
                    </div>
                    <div style={{ display: 'flex', gap: 6, marginBottom: 10 }}>
                      <input
                        style={{ ...styles.input, flex: 1, margin: 0, fontSize: '0.8rem' }}
                        value={newFact}
                        onChange={(event) => setNewFact(event.target.value)}
                        placeholder="Tambah fakta baru..."
                        onKeyDown={(event) => {
                          if (event.key === 'Enter') void handleAddFact(location.id);
                        }}
                      />
                      <button onClick={() => void handleAddFact(location.id)} style={styles.btnPrimary}>
                        <Plus size={14} />
                      </button>
                    </div>
                    {facts.length === 0 ? (
                      <p style={{ opacity: 0.4, fontSize: '0.8rem' }}>Belum ada fakta.</p>
                    ) : facts.map((fact) => (
                      <div key={fact.id} style={{
                        display: 'flex',
                        gap: 8,
                        alignItems: 'flex-start',
                        marginBottom: 6,
                        background: 'rgba(0,0,0,0.2)',
                        padding: '6px 10px',
                        borderRadius: 6,
                      }}>
                        <span style={{ flex: 1, fontSize: '0.78rem', lineHeight: 1.5 }}>{fact.fact}</span>
                        <button
                          onClick={() => void handleDeleteFact(fact.id, location.id)}
                          style={{ background: 'none', border: 'none', color: '#F44336', cursor: 'pointer', padding: 2, flexShrink: 0 }}
                          title="Hapus fakta"
                        >
                          <Trash2 size={13} />
                        </button>
                      </div>
                    ))}
                  </div>

                  <div>
                    <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: 10, gap: 8 }}>
                      <div>
                        <div style={{ fontWeight: 'bold', color: '#D4AF37', fontSize: '0.9rem' }}>
                          📝 Bank Soal
                        </div>
                        <div style={{ fontSize: '0.72rem', opacity: 0.7 }}>
                          {activeQuestionCount} aktif / {questions.length} total • 10 dipilih per sesi
                        </div>
                      </div>
                      <button
                        onClick={() => {
                          setEditQuestion(null);
                          setShowQuestionModal(true);
                        }}
                        style={styles.btnPrimary}
                      >
                        <Plus size={13} /> Tambah Soal
                      </button>
                    </div>

                    {questions.length === 0 ? (
                      <p style={{ opacity: 0.4, fontSize: '0.8rem' }}>Belum ada soal. Tambahkan soal untuk lokasi ini.</p>
                    ) : questions.map((question, index) => (
                      <div key={question.id} style={{
                        background: 'rgba(0,0,0,0.2)',
                        padding: '8px 12px',
                        borderRadius: 6,
                        marginBottom: 6,
                      }}>
                        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: 8 }}>
                          <span style={{ fontSize: '0.8rem', flex: 1 }}>
                            <span style={{ color: '#D4AF37', marginRight: 6 }}>#{index + 1}</span>
                            {question.question}
                            <span className={`badge ${question.is_active ? 'green' : 'orange'}`} style={{ marginLeft: 8, fontSize: 9 }}>
                              {question.is_active ? 'Aktif' : 'Nonaktif'}
                            </span>
                          </span>
                          <div style={{ display: 'flex', gap: 4, flexShrink: 0 }}>
                            <button
                              onClick={() => {
                                setEditQuestion(question);
                                setShowQuestionModal(true);
                              }}
                              style={{ background: 'none', border: 'none', color: '#D4AF37', cursor: 'pointer' }}
                              title="Edit soal"
                            >
                              <Edit2 size={13} />
                            </button>
                            {question.is_active && (
                              <button
                                onClick={() => void handleDeactivateQuestion(question.id, location.id)}
                                style={{ background: 'none', border: 'none', color: '#F44336', cursor: 'pointer' }}
                                title="Nonaktifkan soal"
                              >
                                <Trash2 size={13} />
                              </button>
                            )}
                          </div>
                        </div>
                        <div style={{ display: 'flex', gap: 4, flexWrap: 'wrap', marginTop: 5 }}>
                          {question.options?.map((option, optionIndex) => (
                            <span
                              key={option.id ?? optionIndex}
                              style={{
                                fontSize: '0.7rem',
                                padding: '1px 6px',
                                borderRadius: 3,
                                background: option.is_correct ? 'rgba(76,175,80,0.3)' : 'rgba(255,255,255,0.05)',
                                border: option.is_correct ? '1px solid #4CAF50' : '1px solid transparent',
                              }}
                            >
                              {String.fromCharCode(65 + optionIndex)}. {option.option_text}
                            </span>
                          ))}
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          )}
        </div>
      ))}

      {showLocationModal && (
        <LocationModal
          loc={editLocation}
          nextLevelOrder={nextLevelOrder}
          onClose={() => setShowLocationModal(false)}
          onSave={async () => {
            setShowLocationModal(false);
            setEditLocation(null);
            await fetchLocations();
          }}
        />
      )}

      {showQuestionModal && activeLocation && (
        <LocationQuestionModal
          locId={activeLocation.id}
          question={editQuestion}
          onClose={() => {
            setShowQuestionModal(false);
            setEditQuestion(null);
          }}
          onSave={async () => {
            setShowQuestionModal(false);
            setEditQuestion(null);
            await refreshLocationQuestions(activeLocation.id);
          }}
        />
      )}
    </div>
  );
};

export default LocationsPage;
