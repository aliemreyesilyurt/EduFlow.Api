# EduFlow Yol Haritası

**Vizyon:** Çok kiracılı (multi-tenant) bir online kurs / LMS platformu. Kurumlar (tenant) kaydolur,
eğitmenler adım adım (veya tek aşamalı) kurs içeriği üretir, öğrenciler kayıt olup dersleri takip
eder, yorum/puan bırakır, kurs sonunda sınava girer ve başarılı olursa sistem içi puan kazanır.

**Kapsam:** Backend (.NET 10, Clean Architecture + Vertical Slice) ve Frontend (Vue.js) birlikte.

**Mevcut durum:** Multi-tenant altyapı, kimlik doğrulama (login/logout/refresh/tenant & öğrenci
kaydı/eğitmen daveti) ve rol yapısı (SysAdmin, TenantAdmin, Instructor, Student) hazır. `Course`
ve `Step` üzerinde tam CRUD + yayınlama/sıralama akışı çalışıyor; öğrenci kaydı (Enrollment) ve
adım ilerlemesi (StepProgress) de eklendi; sırada yorum/değerlendirme (Faz 4) var.

**Öncelik sırası:** Önce auth akışını sağlamlaştırıp Course/Enrollment temelini kurmak (backend
ağırlıklı), ardından frontend'i bu API'lerin üzerine inşa etmek, sonra sınav/puan/bütünlük
sistemleri eklenir. Bağımsız parçalar (ör. frontend iskeleti) daha erken paralel başlatılabilir.

**Teknik not — Sınav Bütünlüğü (Proctoring):** Bir web tarayıcısından OS seviyesinde "başka hangi
uygulamalar/paneller açık" diye kesin tarama yapmak mümkün değildir (bu güvenlik sandbox'ının bir
sonucudur; ancak native/masaüstü bir istemciyle tam yapılabilir). Bu roadmap'te proctoring,
**otomatik diskalifiye değil kanıt toplama** olarak tasarlanır: fullscreen'den çıkış, sekme/pencere
odak kaybı, kopyala-yapıştır girişimi, (destekleyen tarayıcılarda) bağlı ekran sayısı ve periyodik
kamera görüntüsü toplanır; nihai kararı eğitmen bu kanıtlara bakarak verir. Kamera görüntüsü almak
KVKK/GDPR kapsamında açık rıza ve veri saklama politikası gerektirir — bu tenant bazlı yapılandırılır.

---

## Faz 0 — Temizlik & Domain Hazırlığı ✅
**Amaç:** Gerçek domain'e geçmeden önce iskeleti temizlemek ve genişleyen domain'i taslaklamak.

- [x] `Book` feature'ını (entity, handler'lar, endpoint'ler, migration) kaldır
- [x] Domain taslağını çıkar — Faz 2-3'te kullanılacak dört entity gerçekten kodlandı:
      `Course`, `Step`, `Enrollment`, `StepProgress` (+ `CourseStatus`, `StepContentType` enum'ları).
      Kalanı (`Comment`, `Rating`, `Exam`, `Question`, `ExamAttempt`, `ProctoringEvent`,
      `ProctoringSnapshot`, `PointsWallet`, `PointsLedgerEntry`, `PointsRule`) bilinçli olarak
      taslak seviyesinde bırakıldı; her biri kendi fazında (Faz 4, 8, 9, 10) kodlanacak — henüz
      kullanılmayacak tablolarla şema şişirilmedi
- [x] `ApiTags`, `BookErrors` gibi kalıntıları yeni domain'e göre güncelle/temizle — `BookErrors`
      silindi, `ApiTags` içine `Courses`/`Steps`/`Enrollments` eklendi (`Books` kaldırıldı)
- [x] `PolicyNames` içindeki yetkilendirme politikalarını netleştir — sabit isimleri değerleriyle
      hizalandı (`SysAdminOnly`, `TenantAdminOrAbove`, `InstructorOrAbove`, `StudentOrAbove`),
      ayrıca `StudentOnly` (sadece öğrenci rolüyle anlamlı aksiyonlar) ve `Authenticated`
      (rol gerektirmeyen ama oturum isteyen endpoint'ler) eklendi

## Faz 1 — Auth & Tenant Onboarding Sağlamlaştırma *(Backend)* ✅

- [x] Email doğrulama akışı (kayıt sonrası) — `IEmailSender` soyutlaması eklendi; gövdeler
      `{{Token}}` yer tutuculu HTML şablonlarından (ortak layout) render ediliyor, gönderim
      MailKit ile SMTP üzerinden yapılıyor (ayarlar `SystemSettings`'ten okunuyor, host boşsa
      log'a düşüyor — bkz. aşağıdaki not). Identity'de
      `RequireConfirmedEmail = true`; `RegisterTenant`/`RegisterStudent` artık otomatik login
      yapmıyor, sadece kimlik döndürüp doğrulama e-postası gönderiyor (`ConfirmEmail`,
      `ResendConfirmationEmail` endpoint'leri eklendi)
- [x] "Şifremi unuttum" / şifre sıfırlama akışı — `ForgotPassword`/`ResetPassword` endpoint'leri;
      hesap enumeration'a karşı var olmayan e-postada da 204 dönülüyor; şifre değişince
      kullanıcının tüm refresh token'ları iptal ediliyor
- [x] Refresh token rotation ve "tüm cihazlardan çıkış yap" — `RefreshToken`'a FK/unique index
      eklendi, rotasyon zinciri `ReplacedByTokenId` ile izleniyor; iptal edilmiş bir token tekrar
      kullanılırsa (reuse detection) kullanıcının tüm token'ları iptal ediliyor; yeni
      `POST auth/logout-all` endpoint'i tüm cihazlardan çıkışı sağlıyor
- [x] Login'de rate limiting / brute-force koruması — ASP.NET Core rate limiter (IP bazlı,
      anonim auth endpoint'lerinde) + Identity lockout (5 hatalı denemede 15 dakika kilit)
- [x] `InviteInstructor` akışının uçtan uca doğrulanması (davet e-postası, kabul akışı) —
      TenantAdmin artık sadece e-posta/isim giriyor, şifresiz kullanıcı + davet token'ı
      üretiliyor, e-posta gönderiliyor; eğitmen `POST auth/invitations/accept` ile kendi
      şifresini belirleyip daveti kabul ediyor
- [x] Rol bazlı yetkilendirme policy'lerinin endpoint'lere uygulanması — her auth endpoint'i
      açıkça `AllowAnonymous` ya da uygun `PolicyNames` politikasını taşıyor; hata→HTTP durum kodu
      eşlemesi `Error.ToHttpResult()` ile merkezileştirildi

## Faz 2 — Çekirdek Kurs Domain'i: Course + Step *(Backend)* ✅
**Amaç:** Tek aşamalı ya da çok adımlı (step-by-step) kursların temelini kurmak.
*(Not: `Course`/`Step` entity'leri ve şema Faz 0'da hazırlandı; bu fazda CRUD/endpoint katmanı yazılacak.)*

- [x] `Course` entity + CRUD (create/list/detail/update/delete/publish) — `CourseFeature` altında
      `CreateCourse`, `GetCourseById`, `GetAllCourses`, `UpdateCourse`, `DeleteCourse`,
      `PublishCourse` slice'ları eklendi
- [x] `Step` entity (sıralı, kurs tek adımdan da oluşabilir) + CRUD, sıralama desteği —
      `StepFeature` altında `CreateStep`, `GetStepById`, `GetAllSteps`, `UpdateStep`, `DeleteStep`
      eklendi; yeni adım her zaman sona eklenir, `POST courses/{courseId}/steps/reorder` tüm
      sırayı tek seferde günceller (unique `(CourseId, Order)` indeksini ihlal etmemek için
      geçici negatif değerler üzerinden iki adımlı güncelleme yapılır)
- [x] Basit içerik tipi desteği (video linki, döküman linki, metin) — `StepContentType`'a göre
      `ContentUrl`/`TextContent` koşullu olarak zorunlu kılınıyor (FluentValidation); dosya
      yükleme Faz 7'de
- [x] Instructor–Course ilişkisi (bir kursun sahibi/eğitmeni) — `Course.InstructorId` oluşturan
      kullanıcıya set ediliyor; yönetim (update/delete/publish/step CRUD) sahibi eğitmen veya
      TenantAdmin/SysAdmin ile sınırlı (`CourseAccess.CanManage`)
- [x] Tenant-scoped kurs listeleme/detay endpoint'leri — tenant izolasyonu mevcut EF Core global
      query filter'ı ile otomatik; ayrıca taslak/arşiv kurslar sadece sahibi/TenantAdmin/SysAdmin
      tarafından görülebiliyor, yayınlanan kurslar herkese açık (`CourseAccess.CanView`)

## Faz 3 — Enrollment & Öğrenme İlerlemesi *(Backend)* ✅
**Amaç:** Öğrencinin bir kursa kaydolup ilerlemesini takip edebilmesi.
*(Not: `Enrollment`/`StepProgress` entity'leri ve şema Faz 0'da hazırlandı; bu fazda CRUD/endpoint katmanı yazıldı.)*

- [x] `Enrollment` entity (Student ↔ Course) — Faz 0'da eklenen entity/şema kullanıldı
- [x] `EnrollInCourse` / `Unenroll` endpoint'leri — `EnrollmentFeature` altında
      `POST courses/{courseId}/enroll` ve `DELETE courses/{courseId}/enroll`; yalnızca yayınlanan
      kurslara kayıt olunabiliyor, aynı kursa iki kez kayıt engelleniyor (`EnrollmentErrors`)
- [x] `StepProgress`: adım tamamlama takibi, kurs geneli ilerleme yüzdesi —
      `POST steps/{stepId}/complete` öğrencinin kaydı (enrollment) üzerinden tamamlama kaydı
      oluşturuyor (idempotent), tamamlanan/toplam adım sayısından ilerleme yüzdesini hesaplıyor;
      tüm adımlar bitince `Enrollment.CompletedOn` otomatik set ediliyor
- [x] "Kurslarım" (öğrenci) ve "Kayıtlı Öğrenciler" (eğitmen) endpoint'leri —
      `GET students/me/courses` (öğrencinin kendi kayıtları + ilerleme yüzdesi) ve
      `GET courses/{courseId}/students` (kursun sahibi eğitmen veya TenantAdmin/SysAdmin için
      kayıtlı öğrenci listesi + ilerleme), `CourseAccess.CanManage` ile yetkilendirildi

## Faz 4 — Yorum & Değerlendirme *(Backend)*
**Amaç:** Her adıma ve kursun geneline geri bildirim verilebilmesi.

- [ ] `Comment` entity: hem `Step` bazında hem `Course` (kurs geneli) bazında yorum
- [ ] `Rating` entity: kurs geneline 1-5 yıldız puanlama (bir öğrenci bir kursa tek rating)
- [ ] Kurs kartında gösterilecek ortalama puan / yorum sayısı aggregate hesaplama
- [ ] Yorum moderasyonu (eğitmen/tenantAdmin tarafından gizleme/şikayet) — temel seviye

## Faz 5 — Frontend Temelleri *(Vue.js)*
**Amaç:** Frontend iskeletini kurmak ve auth akışını bağlamak.

- [ ] Vue 3 + Vite + Pinia + Vue Router proje kurulumu
- [ ] Auth ekranları: Login, Tenant Kaydı, Öğrenci Kaydı, Şifremi Unuttum
- [ ] HTTP client (Axios) + token refresh interceptor + rol bazlı route guard
- [ ] Rol bazına göre farklı shell/layout (TenantAdmin / Instructor / Student)

## Faz 6 — Frontend: Öğrenci Deneyimi
**Amaç:** Öğrencinin kurs keşfedip takip edebilmesi ve geri bildirim verebilmesi.

- [ ] Kurs kataloğu (listeleme, arama, filtre, ortalama puan gösterimi)
- [ ] Kurs detay sayfası ve kayıt ol (enroll) akışı
- [ ] "Kurslarım" dashboard + step görüntüleyici (video/döküman) + ilerleme çubuğu
- [ ] Step'e yorum bırakma, kurs geneline yorum + yıldız puanlama ekranı

## Faz 7 — Eğitmen Paneli & İçerik Yönetimi *(Backend + Frontend)*
**Amaç:** Eğitmenlerin gerçek içerik üretebilmesi.

- [ ] Backend: dosya/video yükleme (S3/Azure Blob uyumlu storage soyutlaması)
- [ ] Backend: Course/Step yönetim endpoint'lerinin genişletilmesi
- [ ] Frontend: Kurs oluşturma sihirbazı, step ekleme/sıralama, içerik yükleme arayüzü
- [ ] Kurs yayınlama akışı (draft → published)
- [ ] Frontend: Yorum moderasyon ekranı (eğitmen/tenantAdmin)

## Faz 8 — Sınav Sistemi *(Backend + Frontend)*
**Amaç:** Eğitmenin belirlediği şekilde, kursu bitiren öğrencinin sınava tabi tutulması.

- [ ] Backend: `Exam` entity — kursa bağlı, eğitmen opsiyonel olarak tanımlar (sınavsız kurs da olabilir)
- [ ] Backend: `Question`/seçenekler (çoktan seçmeli minimum), süre sınırı, geçme notu (eğitmen tanımlı)
- [ ] Backend: `ExamAttempt` + cevaplar, otomatik puanlama, deneme hakkı limiti
- [ ] Backend: sınav geçilirse "puan kazanım talebi" oluşturma (kesin ödül Faz 9 onayına bağlı)
- [ ] Frontend: Eğitmen için sınav/soru oluşturma ekranı, öğrenci için sınav çözme ekranı, sonuç sayfası

## Faz 9 — Sınav Bütünlüğü (Proctoring) *(Backend + Frontend)*
**Amaç:** Sınav sırasında kanıt toplamak; nihai hile kararını eğitmene bırakmak.

- [ ] Frontend: Sınav girişinde fullscreen zorunluluğu + fullscreen'den çıkış tespiti/loglama
- [ ] Frontend: Kopyala/yapıştır/sağ tık engelleme + girişim loglama (best-effort, garanti değil)
- [ ] Frontend: Sekme/pencere odak kaybı tespiti (`visibilitychange`/`blur`)
- [ ] Frontend: Desteklenen tarayıcılarda bağlı ekran sayısı sinyali (Window Management API, best-effort)
- [ ] Frontend: Kullanıcı rızası alınarak belirli aralıklarla kameradan anlık görüntü alma ve yükleme
- [ ] Backend: `ProctoringEvent` log (her ihlal/sinyal) + `ProctoringSnapshot` storage
- [ ] Backend: KVKK/GDPR uyumlu açık rıza metni akışı + tenant bazlı veri saklama süresi/silme politikası
- [ ] Frontend: Eğitmen için "sınav bütünlük raporu" (ihlal zaman çizelgesi + snapshot galerisi)
- [ ] Backend: Eğitmen onay/red aksiyonu — onaylanırsa Faz 8'deki puan kazanım talebi kesinleşir

## Faz 10 — Puan (Points) Ekonomisi *(Backend + Frontend, tenant bazlı yapılandırılabilir)*
**Amaç:** Sınavı geçip eğitmen onayı alan öğrencinin kazandığı puanın site içinde efektif kullanımı.

- [ ] Backend: `PointsWallet` / `PointsLedgerEntry` (kazanım-harcama hareket geçmişi)
- [ ] Backend: `PointsRule` — TenantAdmin panelinden "puan ile ne yapılabilir" tanımı (ör. sertifika
      unlock, öncelikli destek, indirim kodu, liderlik tablosu rozeti — tenant'a göre değişir)
- [ ] Backend: puan kazanım miktarı sınav/kurs bazında eğitmen tarafından tanımlanabilir
- [ ] Frontend: TenantAdmin için puan kuralları konfigürasyon ekranı
- [ ] Frontend: Öğrenci için puan bakiyesi, hareket geçmişi ve "puanla neler yapılabilir" ekranı

## Faz 11 — Bildirim, Sertifika & Raporlama
**Amaç:** Platformu tamamlayan destek özellikler.

- [ ] Email/in-app bildirimler (kayıt, sınav sonucu, puan kazanımı, kurs tamamlama)
- [ ] Kurs+sınav tamamlandığında otomatik sertifika üretimi (PDF)
- [ ] TenantAdmin için temel raporlama (aktif öğrenci, tamamlanma oranı, ortalama puan, sınav başarı oranı)

## Faz 12 — Prod Hazırlığı & Operasyon
**Amaç:** Canlıya alım için sağlamlaştırma.

- [ ] CI/CD pipeline (build/test/deploy)
- [ ] Structured logging & monitoring (health check'lerin genişletilmesi, log toplama)
- [ ] Docker Compose'dan prod-ready deployment'a geçiş
- [ ] Bağımlılık/güvenlik taraması, secrets yönetimi, snapshot storage için erişim kontrolü
- [ ] Temel yük/performans testi

## Faz 13 — Monetizasyon *(Opsiyonel, sonraki dönem)*
- [ ] Abonelik/ödeme entegrasyonu (Stripe/iyzico)
- [ ] Kurs fiyatlandırma, kupon/indirim sistemi

---

## Açık Sorular / Riskler
- Sınav zorunlu mu her kursta, yoksa eğitmen "sınavsız tamamlanabilir" seçeneği sunabilir mi? (Şu an: opsiyonel varsayıldı)
- Kamera görüntüsü saklama süresi ve kimlerin erişebileceği (sadece ilgili eğitmen mi, TenantAdmin de mi?) netleşmeli
- Proctoring sinyalleri (fullscreen çıkışı, odak kaybı vb.) belirli bir eşiği aşınca otomatik uyarı mı gösterilecek, yoksa sadece pasif loglanıp rapor mu sunulacak?
- İçerik depolama için hangi storage sağlayıcısı kullanılacak (S3, Azure Blob, yerel disk)?
- Video içerik için transcoding/streaming ihtiyacı olacak mı, yoksa harici bir servise (YouTube/Vimeo embed) mi dayanılacak?
- Puan ile yapılabilecekler listesi ilk sürümde hangi somut örneklerle başlayacak (MVP için 1-2 tenant kuralı yeterli mi)?

---

Bu doküman onaylandıktan sonra Faz 0'dan başlayarak ilerleyebiliriz. Fazların sırası veya
kapsamı üzerinde değişiklik istersen belirt, güncelleyeyim.
