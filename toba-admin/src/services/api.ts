import axios from 'axios';

const api = axios.create({
  baseURL: 'http://localhost:8080/api',
  headers: { 'Content-Type': 'application/json' }
});

// ── STATS & PLAYERS ───────────────────────────────────────
export const getAdminStats  = () => api.get('/admin/stats');
export const getAllPlayers   = () => api.get('/admin/players');

// ── LOKASI ────────────────────────────────────────────────
export const adminGetLocations    = () => api.get('/admin/locations');
export const adminCreateLocation  = (data: any) => api.post('/admin/locations', data);
export const adminUpdateLocation  = (id: number, data: any) => api.put(`/admin/locations/${id}`, data);
export const adminDeleteLocation  = (id: number) => api.delete(`/admin/locations/${id}`);
export const adminUploadLocationImage = (file: File) => {
  const fd = new FormData(); fd.append('image', file);
  return api.post('/admin/locations/upload-image', fd, { headers: { 'Content-Type': 'multipart/form-data' }});
};

// ── FAKTA BUDAYA ──────────────────────────────────────────
export const adminGetFacts    = (locId: number) => api.get(`/admin/locations/${locId}/facts`);
export const adminCreateFact  = (locId: number, fact: string) => api.post(`/admin/locations/${locId}/facts`, { fact });
export const adminDeleteFact  = (id: number) => api.delete(`/admin/facts/${id}`);

// ── SOAL JELAJAH ──────────────────────────────────────────
export const adminGetLocQuestions   = (locId: number) => api.get(`/admin/locations/${locId}/questions`);
export const adminCreateLocQuestion = (locId: number, data: any) => api.post(`/admin/locations/${locId}/questions`, data);
export const adminUpdateLocQuestion = (id: number, data: any) => api.put(`/admin/questions/location/${id}`, data);
export const adminDeleteLocQuestion = (id: number) => api.delete(`/admin/questions/location/${id}`);

// ── TRIVIA KATEGORI ───────────────────────────────────────
export const adminGetTriviaCategories   = () => api.get('/admin/trivia/categories');
export const adminCreateTriviaCategory  = (data: any) => api.post('/admin/trivia/categories', data);
export const adminUpdateTriviaCategory  = (id: number, data: any) => api.put(`/admin/trivia/categories/${id}`, data);
export const adminDeleteTriviaCategory  = (id: number) => api.delete(`/admin/trivia/categories/${id}`);

// ── SOAL TRIVIA ───────────────────────────────────────────
export const adminGetTriviaQuestions   = (catId: number) => api.get(`/admin/trivia/${catId}/questions`);
export const adminCreateTriviaQuestion = (catId: number, data: any) => api.post(`/admin/trivia/${catId}/questions`, data);
export const adminUpdateTriviaQuestion = (id: number, data: any) => api.put(`/admin/trivia/questions/${id}`, data);
export const adminDeleteTriviaQuestion = (id: number) => api.delete(`/admin/trivia/questions/${id}`);

// ── SOAL BATTLE ───────────────────────────────────────────
export const adminGetBattleQuestions   = () => api.get('/admin/battle/questions');
export const adminCreateBattleQuestion = (data: any) => api.post('/admin/battle/questions', data);
export const adminUpdateBattleQuestion = (id: number, data: any) => api.put(`/admin/battle/questions/${id}`, data);
export const adminDeleteBattleQuestion = (id: number) => api.delete(`/admin/battle/questions/${id}`);

// ── ACHIEVEMENT ───────────────────────────────────────────
export const adminGetAchievements   = () => api.get('/admin/achievements');
export const adminCreateAchievement = (data: any) => api.post('/admin/achievements', data);
export const adminUpdateAchievement = (id: number, data: any) => api.put(`/admin/achievements/${id}`, data);
export const adminDeleteAchievement = (id: number) => api.delete(`/admin/achievements/${id}`);

// ── SETTINGS ──────────────────────────────────────────────
export const adminGetSettings    = () => api.get('/admin/settings');
export const adminUpdateSetting  = (key: string, value: string) => api.put(`/admin/settings/${key}`, { value });

// ── MUSIK ─────────────────────────────────────────────────
export const adminGetMusic    = () => api.get('/admin/music');
export const adminUploadMusic = (file: File) => {
  const fd = new FormData(); fd.append('music', file);
  return api.post('/admin/music/upload', fd, { headers: { 'Content-Type': 'multipart/form-data' }});
};
export const adminDeleteMusic = (filename: string) => api.delete(`/admin/music/${filename}`);

// ── GALERI ────────────────────────────────────────────────
export const adminGetGallery   = () => api.get('/admin/gallery');
export const adminCreateGallery = (data: any) => api.post('/admin/gallery', data);
export const adminUpdateGallery = (id: number, data: any) => api.put(`/admin/gallery/${id}`, data);
export const adminDeleteGallery = (id: number) => api.delete(`/admin/gallery/${id}`);

export const getLeaderboard = () => api.get('/leaderboard');
export default api;