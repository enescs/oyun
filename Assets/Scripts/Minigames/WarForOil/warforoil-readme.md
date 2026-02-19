# War For Oil — Minigame Sistemi

## Genel Bakis

Oyuncu, petrol kaynaklari zengin ulkeleri secip hukumetine baski yaparak savas baslatir. Savas surecinde gelen eventlere yanit vererek halk destegini yonetir. Savas sonunda olasilik tabanli bir kontrol yapilir — kazanilirsa ulkenin kaynaklari ele gecirilir, kaybedilirse agir cezalar uygulanir ve **minigame kalici olarak devre disi kalir**.

Savas sirasinda **event zincirleri** (hukumet fonlama krizi gibi), **rakip isgal** (baska bir ulkenin ayni hedefe saldirmasi), **toplum tepkisi** (savas karsiti gosteriler) ve **vandalizm** (pasif gelir urunlerine saldiri) tetiklenebilir.

---

## Mimari

Sistem 4 ScriptableObject + 1 Manager + yardimci siniflardan olusur:

```
WarForOilDatabase (SO)          — tum ayarlar, event havuzlari ve ulke havuzu
  ├── WarForOilCountry (SO)     — tek bir ulke verisi
  ├── WarForOilEvent (SO)       — savas sirasi event (normal / zincir / kose kapma / rakip teklif)
  │     └── WarForOilEventChoice (Serializable) — event secenegi
  ├── ChainRole (enum)          — event zincir rolu (None / Head / Link)
  └── RefusalThreshold (Serializable) — zincir reddetme esikleri

WarForOilManager (MonoBehaviour, Singleton) — ana mantik
WarForOilResult (Serializable)              — savas sonucu verisi
WarForOilState (enum)                       — durum makinesi
```

Asset olusturma: `Assets → Create → Minigames → WarForOil → Database / Country / Event`

---

## Durum Makinesi

```
Idle ──→ PressurePhase ──→ WarProcess ←──→ EventPhase
              │                │  ↑              │
              ↓                │  │              ↓
         (CancelPressure)      │  └── ChainWaiting
              │                │
              ↓                ↓
            Idle          ResultPhase ──→ Idle
```

| Durum | Aciklama |
|-------|----------|
| **Idle** | Minigame bosta. Ulke rotasyonu devam eder. |
| **CountrySelection** | (Rezerve) Ilerde kullanilabilir. Su an Idle'dan dogrudan PressurePhase'e gecilir. |
| **PressurePhase** | Ulke secildi. Oyuncu "Baski Yap" butonuyla siyasi nufuza dayali basari kontrolu yapar. Basarisizsa cooldown. |
| **WarProcess** | Savas baslamis. Timer ilerler, belirli araliklarda eventler tetiklenir. |
| **EventPhase** | Savas sirasinda event geldi. Oyun duraklatilir, oyuncu karar verir veya sure dolar. |
| **ChainWaiting** | Zincir eventi bekleniyor. Oyun calisir ama savas timer'i durur. chainInterval sonra sonraki event tetiklenir. |
| **ResultPhase** | Savas bitti, sonuc ekrani gosteriliyor. Oyun duraklatilmis. UI ekrani kapatinca stat'lar uygulanir. |

---

## Veri Siniflari

### WarForOilDatabase

Tum minigame ayarlarinin tek noktadan yonetildigi ScriptableObject.

| Alan | Varsayilan | Aciklama |
|------|-----------|----------|
| **Ulkeler** | | |
| `countries` | — | Tum ulke havuzu |
| **Ulke Rotasyonu** | | |
| `visibleCountryCount` | 3 | UI'da ayni anda goruntulenen ulke sayisi |
| `rotationInterval` | 90 sn | Ulke degisim araligi |
| **Baski Ayarlari** | | |
| `pressureCooldown` | 20 sn | Basarisiz baski sonrasi bekleme |
| `politicalInfluenceMultiplier` | 0.01 | Siyasi nufuzun basari sansina carpani |
| **Savas Eventleri** | | |
| `events` | — | Savas sirasinda tetiklenen normal event havuzu |
| **Savas Ayarlari** | | |
| `warDuration` | 300 sn | Savas suresi (5 dakika) |
| `eventInterval` | 15 sn | Event kontrol araligi |
| `initialSupportStat` | 50 | Destek stat baslangic degeri |
| **Sonuc Ayarlari** | | |
| `baseWinChance` | 0.375 | Temel kazanma sansi |
| `supportWinBonus` | 0.625 | Tam destegin kazanma sansina max katkisi |
| `supportRewardRatio` | 0.8 | Support 100 olsa bile baseReward'in max bu orani alinir |
| `minWinChance` | 0.1 | Minimum kazanma sansi (%10) |
| `maxWinChance` | 0.9 | Maximum kazanma sansi (%90) |
| **Ateskes Ayarlari** | | |
| `ceasefireMinSupport` | 40 | Ateskes yapabilmek icin minimum destek degeri |
| `ceasefirePenalty` | 100 | En kotu ateskesteki para kaybi |
| `ceasefireMaxReward` | 200 | En iyi ateskesteki max kazanc carpani |
| **Odul/Ceza Ayarlari** | | |
| `warLossPenalty` | 200 | Kaybetme para cezasi |
| `warLossPoliticalPenalty` | 20 | Kaybetme siyasi nufuz dususu |
| `warLossSuspicionIncrease` | 15 | Kaybetme suphe artisi |
| **Rakip Isgal Ayarlari** | | |
| `rivalInvasionMinWarTime` | 60 sn | Rakip isgalin en erken tetiklenebilecegi savas suresi |
| `rivalInvasionChance` | 0.3 | Her event check'te rakip isgal tetiklenme sansi |
| `rivalDealRewardRatio` | 0.6 | Anlasma kabul edilince oyuncuya kalan odul orani |
| `rivalDealEndDelay` | 10 sn | Anlasma kabul edilince savas bitis gecikmesi |
| `initialCornerGrabStat` | 50 | Kose kapma stat baslangic degeri (0-100) |
| `rivalOfferEvent` | — | Rakip isgal teklif event'i (tek bir event, tum rakipler icin ortak) |
| `cornerGrabEvents` | — | Kose kapma yarisi event havuzu |
| **Toplum Tepkisi Ayarlari** | | |
| `protestMinWarTime` | 90 sn | Toplum tepkisinin en erken tetiklenebilecegi savas suresi |
| `protestChance` | 0.25 | Her event check'te toplum tepkisi tetiklenme sansi |
| `initialProtestStat` | 30 | Toplum tepkisi baslangic degeri (0-100) |
| `protestFailThreshold` | 80 | Bu degerin ustunde savas otomatik ateskese baglanir |
| `protestSuccessThreshold` | 10 | Bu degerin altina dusurulurse tepki bastirilmis sayilir |
| `protestDriftInterval` | 3 sn | Pasif drift tick araligi |
| `protestDriftDivisor` | 10 | drift = son choice modifier / divisor (her tick'te) |
| `protestTriggerEvent` | — | Toplum tepkisi baslangic event'i |
| `protestEvents` | — | Toplum tepkisi event havuzu |
| **Vandalizm Ayarlari** | | |
| `vandalismDamageInterval` | 5 sn | Vandalizm hasar tick araligi |
| `vandalismLightDamage` | 5 | Light seviyede tick basina wealth kaybi |
| `vandalismModerateDamage` | 15 | Moderate seviyede tick basina wealth kaybi |
| `vandalismHeavyDamage` | 30 | Heavy seviyede tick basina wealth kaybi |
| `vandalismSevereDamage` | 50 | Severe seviyede tick basina wealth kaybi |

### WarForOilCountry

Her ulke icin ayri bir ScriptableObject asset'i olusturulur.

| Alan | Aciklama |
|------|----------|
| `id` | Benzersiz kimlik |
| `displayName` | UI'da gorunecek ulke adi |
| `description` | Ulke aciklamasi (TextArea) |
| `baseReward` | Savas kazanildiginda taban odul |
| `invasionDifficulty` | 0-1 arasi, isgal zorlugu → kazanma sansini dusurur |

**Not:** Eventler ulke uzerinde degil, Database uzerinde tanimlanir. Tum ulkeler ayni event havuzunu paylasir.

### WarForOilEvent

Savas sirasinda tetiklenen karar olaylari. Ayni event sinifi normal eventler, zincir eventleri, kose kapma eventleri ve rakip teklif event'i icin kullanilir. Eventin hangi havuza ait oldugu Database'deki listeye gore belirlenir.

| Alan | Aciklama |
|------|----------|
| `id` | Benzersiz kimlik |
| `displayName` | Event basligi |
| `description` | Event aciklamasi (TextArea) |
| `devNote` | Sadece Inspector'da gorunen gelistirici notu (oyuna etkisi yok) |
| `minWarTime` | Bu event savas basladiginda en az kac saniye sonra gelebilir |
| `decisionTime` | Karar suresi (varsayilan 10 sn) |
| `isRepeatable` | Ayni savasta tekrar tetiklenebilir mi |
| `maxRepeatCount` | En fazla kac kez tekrar edebilir (isRepeatable true ise) |
| `choices` | Secenek listesi |
| `defaultChoiceIndex` | Sure dolunca secilecek secenek (-1 = ilk available secenek) |
| **Zincir Ayarlari** | |
| `chainRole` | None / Head / Link (asagida detayli) |
| `nextChainEvent` | Sonraki zincir event'i (null = zincirin sonu) |
| `chainInterval` | Sonraki event'e kadar bekleme suresi (saniye) |
| `skillsToLock` | Zincir bittiginde kilitlenecek skill'ler (sadece Head'de) |
| `chainFine` | Zincir coktuğunde kesilecek para cezasi (sadece Head'de) |
| `refusalThresholds` | Support'a gore kac reddetmede zincir coker (sadece Head'de) |

#### ChainRole Enum

| Deger | Aciklama |
|-------|----------|
| `None` | Normal event, zincir disi |
| `Head` | Zincirin baslangic event'i — tum zincir config'i (skillsToLock, chainFine, refusalThresholds) burada tanimlanir |
| `Link` | Ara zincir event'i — sadece nextChainEvent ve chainInterval gosterilir |

Inspector'da ChainRole'e gore farkli alanlar gosterilir. Head secilince tum zincir ayarlari, Link secilince sadece baglanti alanlari gorunur.

### WarForOilEventChoice

Event icindeki tek bir secenek. Serializable sinif.

| Alan | Aciklama |
|------|----------|
| **Temel Alanlar** | |
| `displayName` | Secenek adi |
| `description` | Secenek aciklamasi (TextArea) |
| `supportModifier` | Destek stat degisimi (+ = destek artar) |
| `suspicionModifier` | Suphe degisimi |
| `reputationModifier` | Itibar degisimi (+ = artar, - = duser) |
| `politicalInfluenceModifier` | Politik nufuz degisimi (- = dusurur) |
| `costModifier` | Maliyet degisimi (int) |
| `cornerGrabModifier` | Kose kapma stat degisimi (+ = bizim lehimize, sadece yaris aktifken uygulanir) |
| `protestModifier` | Toplum tepkisi stat degisimi (+ = tepki artar, - = azalir, sadece tepki aktifken uygulanir). Ayrica sonraki drift hizini belirler. |
| `hasProtestChance` | true ise protestModifier yerine olasilik bazli sistem kullanilir |
| `protestDecreaseChance` | Azalma ihtimali (0-1, orn. 0.6 = %60) |
| `protestDecreaseAmount` | Azalma miktari (pozitif deger, otomatik cikarilir) |
| `protestIncreaseAmount` | Artma miktari (pozitif deger, otomatik eklenir) |
| **Diger Sonuclar** (foldout) | |
| `endsWar` | Bu secenek savasi bitirir mi |
| `warEndDelay` | Savas kac saniye sonra biter (0 = aninda) |
| `reducesReward` | Odulu dusurur mu |
| `baseRewardReduction` | Base reward'i bu oranda dusurur (0.3 = %30 dusus) |
| `endsWarWithDeal` | Savasi anlasmayla bitirir (garanti odul) |
| `dealDelay` | Anlasma kac saniye sonra savasi bitirir |
| `dealRewardRatio` | Normal kazanimin bu orani garanti verilir (0.8 = %80) |
| `blocksEvents` | Secilirse savas sonuna kadar yeni event gelmez |
| `freezesFeed` | Secilince sosyal medya feed'ini dondurur (SocialMediaManager.TryFreezeFeed) |
| `hasFeedOverride` | Feed'i Militarizm konusuna yonlendirir (SocialMediaManager.SetEventOverride) |
| `feedOverrideRatio` | Yonlendirme orani (0-1, orn. 0.8 = %80) |
| `feedOverrideDuration` | Yonlendirme suresi (saniye) |
| **Zincir Flagleri** (foldout) | |
| `continuesChain` | Zinciri devam ettirir (fonlama) |
| `isChainRefusal` | Zincirde reddetme sayacini artirir |
| `triggersCeasefire` | Zincirden ateskes tetikler (minSupport kontrolu yok) |
| **Rakip Isgal Flagleri** (foldout) | |
| `acceptsRivalDeal` | Rakip isgal anlasmasini kabul eder |
| `rejectsRivalDeal` | Rakip isgal anlasmasini reddeder → kose kapma yarisi baslar |
| **Vandalizm Etkisi** (foldout) | |
| `affectsVandalism` | Bu choice vandalizm seviyesini degistirir mi |
| `vandalismChangeType` | Direct (hedef seviye ata) veya Relative (+/- tik kaydirma) |
| `vandalismTargetLevel` | Direct modda: hedef VandalismLevel (None/Light/Moderate/Heavy/Severe/Ended) |
| `vandalismLevelDelta` | Relative modda: seviye degisimi (orn. +2 = 2 tik artir, -1 = 1 tik azalt) |
| **On Kosullar** (foldout) | |
| `requiredSkills` | Bu secenek icin acilmis olmasi gereken skill'ler |
| `statConditions` | Bu secenek icin saglanmasi gereken stat kosullari |

### WarForOilResult

Savas sonucu. Manager tarafindan olusturulur, event'lerle UI'a iletilir.

| Alan | Aciklama |
|------|----------|
| `country` | Savas yapilan ulke |
| `warWon` | Kazanildi mi |
| `wasCeasefire` | Ateskes mi yapildi |
| `wasDeal` | Anlasmayla mi bitti |
| `wasChainCollapse` | Zincir cokusuyle mi bitti |
| `wasCornerGrabRace` | Kose kapma yarisi miydi |
| `wasProtestCeasefire` | Toplum tepkisi yuzunden ateskes mi |
| `rivalCountry` | Rakip ulke (varsa) |
| `rivalRewardGain` | Rakip ulkenin kazandigi bonus reward |
| `finalSupportStat` | Savas sonu destek degeri |
| `finalProtestStat` | Toplum tepkisi son degeri (aktifse) |
| `finalVandalismLevel` | Savas sonu vandalizm seviyesi (VandalismLevel enum) |
| `winChance` | Hesaplanan kazanma sansi |
| `wealthChange` | Para degisimi (+ kazanc, - kayip) |
| `suspicionChange` | Suphe degisimi |
| `reputationChange` | Itibar degisimi |
| `politicalInfluenceChange` | Siyasi nufuz degisimi |

---

## Event Zinciri Sistemi

Bazi eventler birbirine bagli zincirler olusturur (ornegin hukumet fonlama krizi). Zincir, ScriptableObject referanslari ile singly linked list seklinde kurulur.

### Inspector'da Zincir Kurulumu

1. Ilk event'in `chainRole`'unu **Head** yap
2. Head event'te `nextChainEvent`'e ikinci event'i ata, `chainInterval` (ornegin 5 sn) ayarla
3. Head event'te `skillsToLock`, `chainFine`, `refusalThresholds` doldur
4. Ara event'lerin `chainRole`'unu **Link** yap, `nextChainEvent` ve `chainInterval` ayarla
5. Son event'in `nextChainEvent`'ini **bos birak** (null = zincirin sonu)
6. Her event'in choice'larinda `continuesChain`, `isChainRefusal`, `triggersCeasefire` flaglerini ayarla

### Zincir Akisi

1. Head event tetiklenir → `StartChain()` cagirilir
2. Oyuncu secer:
   - `continuesChain` → sonraki event kuyruklanir (`chainInterval` sonra gelir)
   - `isChainRefusal` → reddetme sayaci artar, threshold asildiysa zincir coker
   - `triggersCeasefire` → zincirden ateskes (minSupport kontrolu YAPILMAZ)
3. Sonraki event `null` ise → zincirin sonu, hukumet dususu (ceza yok, sadece skill lock)

### Zincir Cokusu

Zincir 2 sekilde coker:
- **Fazla reddetme**: chainRefusalCount >= maxRefusals (support'a gore belirlenir) → ceza kesilir + skill'ler kilitlenir
- **Hukumet dususu**: Son event'e kadar devam edildi, nextChainEvent null → ceza yok, sadece skill lock

Cokuste:
- `chainStartEvent.skillsToLock` listesindeki skill'ler `SkillTreeManager.RelockSkill()` ile geri kilitlenir
- `chainStartEvent.chainFine` kadar maliyet eklenir (hukumet dususunde eklenmez)
- Savas kaybedilmis sayilir → kalici devre disi

### RefusalThreshold

Support araligina gore izin verilen reddetme sayisini tanimlar.

| Alan | Aciklama |
|------|----------|
| `minSupport` | Araligin alt siniri (dahil) |
| `maxSupport` | Araligin ust siniri (haric) |
| `maxRefusals` | Bu aralikta izin verilen max reddetme sayisi |

Ornek: support 0-30 → 1 ret, support 30-60 → 2 ret, support 60-100 → 3 ret

### Zincir Sirasinda Oyun Davranisi

- Savas timer'i **durur** (ChainWaiting state'inde warTimer ilerlemez)
- Oyun **calisir** (Time.deltaTime ile chainTimer geri sayar)
- Diger sistemlerin event gondermesi **engellenir** (`EventCoordinator.LockEvents`)
- Event paneli gorunurken oyun **duraklatilir** (EventPhase'de)

---

## Rakip Isgal ve Kose Kapma Sistemi

Savas sirasinda baska bir ulke ayni hedef ulkeye saldirabilir. Bu durumda oyuncuya anlasma teklifi yapilir.

### Tetiklenme Kosullari

- `warTimer >= rivalInvasionMinWarTime` (varsayilan 60 sn)
- Bu savasta henuz rakip isgal tetiklenmemis (`rivalInvasionTriggered == false`)
- Zincir aktif degil ve kose kapma yarisi aktif degil
- Her event check'te `rivalInvasionChance` (varsayilan %30) ihtimalle tetiklenir

### Rakip Ulke Secimi

`database.countries` listesinden rastgele secilir. Su an isgal edilen ulke (`selectedCountry`) ve daha once fethedilen ulkeler (`conqueredCountries`) haric tutulur. Havuzda uygun ulke yoksa rakip isgal tetiklenmez.

### Anlasma Teklifi

Database'deki tek `rivalOfferEvent` event'i gosterilir. Choice'lardaki flagler:
- `acceptsRivalDeal` → anlasmayi kabul et
- `rejectsRivalDeal` → anlasmayi reddet

### Anlasma Kabul Edilirse

1. `eventsBlocked = true` → savas sonuna kadar **hicbir event gelmez**
2. Savas suresi `warDuration - rivalDealEndDelay` kadar ileri sarilir (hizli bitis)
3. Garanti odul: `baseReward * rivalDealRewardRatio` (varsayilan %60 oyuncuya)
4. Rakip ulkenin payi (`baseReward * (1 - rivalDealRewardRatio)`) rakip ulkenin `bonusRewards`'una eklenir
5. Ileride o ulke isgal edilirse bonus reward da alinir

### Anlasma Reddedilirse → Kose Kapma Yarisi

1. `isCornerGrabRace = true` olur
2. `cornerGrabStat` baslangic degerine set edilir (varsayilan 50)
3. Event havuzu `database.cornerGrabEvents`'e gecis yapar (normal eventler yerine)
4. Event trigger sayaclari sifirlanir (yeni havuz icin)
5. Oyuncunun event secimleri `cornerGrabModifier` ile cornerGrabStat'i etkiler

### Kose Kapma Stat (0-100)

- 100 = tamamen bizim lehimize → tum base reward bize
- 0 = tamamen rakip lehine → tum base reward rakibe
- 50 = esit paylasim

Savas sonunda (kazanildiysa):
```
grabRatio = cornerGrabStat / 100
oyuncuOdulu = baseReward * grabRatio * supportRewardRatio
rakipPayi = baseReward * (1 - grabRatio) → bonusRewards'a eklenir
```

### bonusRewards (Kalici)

Rakip isgallerden ulkelere eklenen bonus odulller `Dictionary<WarForOilCountry, float>` olarak saklanir. Savaslar arasi **kalici**dir — sifirlanmaz. Bir ulke isgal edildiginde `GetEffectiveBaseReward()` ile baseReward + bonus hesaplanir.

---

## Toplum Tepkisi Sistemi

Savas sirasinda "savas karsiti gosteriler basladi" seklinde bir toplum tepkisi tetiklenebilir. Oyuncunun gorevi tepkiyi dizginlemek — basaramazsa savas otomatik ateskese baglanir.

### Iki Asamali Tetikleme

**Faz 1 — Foreshadowing:**
- Event check zamani gelince sistem toplum tepkisi icin zar atar
- Basarili olursa `protestPending = true` set edilir
- Oyuncuya event **gosterilmez** — sadece `OnProtestForeshadow` event'i tetiklenir
- UI bu event'i dinleyerek feed'i savas karsiti gonderilere cevirebilir
- Bu cycle tuketilir, normal event secimi yapilmaz

**Faz 2 — Aktivasyon:**
- Bir sonraki event check'te `protestPending` kontrol edilir
- `protestTriggerEvent` oyuncuya gosterilir (baslangic event'i)
- `protestStat` baslangic degerine set edilir (varsayilan 30)
- Toplum tepkisi aktif olur

### Tetiklenme Kosullari

- `warTimer >= protestMinWarTime` (varsayilan 90 sn)
- Bu savasta henuz toplum tepkisi tetiklenmemis (`protestTriggered == false`)
- Zincir aktif degil
- Her event check'te `protestChance` (varsayilan %25) ihtimalle tetiklenir

### Cift Havuzlu Event Sistemi

Kose kapmadan farkli olarak havuz **degismez**, ek havuz devreye girer:
- Aktif havuz (normal veya cornerGrab) calismaya devam eder
- Ek olarak `database.protestEvents` havuzundan da eventler gelir
- Her event check'te her iki havuzdan uygun eventler birlestirilir, rastgele biri secilir

### Pasif Drift

Son secilen choice'un `protestModifier`'ina gore stat pasif olarak kayar:
- Her `protestDriftInterval` (varsayilan 3 sn) saniyede bir drift uygulanir
- `driftPerTick = lastProtestModifier / protestDriftDivisor`
- Ornek: choice +3 verdiyse → her 3sn'de +0.3 eklenir
- Ornek: choice -5 verdiyse → her 3sn'de -0.5 cikarilir
- Ilk choice'a kadar drift 0'dir (pasif degisim yok)

### Basarisizlik — protestStat >= protestFailThreshold (80)

- Savas otomatik ateskese baglanir (`ProtestForceCeasefire`)
- Mevcut ateskes formulune gore odul/ceza hesaplanir
- `wasProtestCeasefire = true` olarak isaretlenir

### Basari — protestStat < protestSuccessThreshold (10)

- Toplum tepkisi havuzundan event gelmesi durur
- Bu savas boyunca toplum tepkisi bir daha tetiklenmez
- Pasif drift durur
- Normal savas akisina devam edilir

### Choice Mekanikleri

Toplum tepkisi eventlerindeki choice'lar:
- **Feed dondurma**: Feed freeze skill'i acilmissa secilebilir (requiredSkills ile kontrol)
- **Mafya ile oldurme**: Ilgili mafya skill'i acilmissa secilebilir (requiredSkills ile kontrol)
- **Gosterilere izin verme**: Genel secenek (herkes secebilir)
- Her choice'un `protestModifier`'i var — hem aninda uygulanir hem sonraki drift hizini belirler

---

## Vandalizm Sistemi

Toplum tepkisi eventleri sirasinda vandallar oyuncunun pasif gelir urunlerine saldirabilir. Vandalizm surekli bir stat degil, kesikli seviye (enum) olarak yonetilir.

### VandalismLevel Enum

| Seviye | Numerik | Aciklama |
|--------|---------|----------|
| `None` | — | Vandalizm baslamadi |
| `Light` | 1 | Hafif hasar |
| `Moderate` | 2 | Orta hasar |
| `Heavy` | 3 | Agir hasar |
| `Severe` | 4 | Siddetli hasar |
| `Ended` | — | Vandalizm bastirildi/bitti |

### Seviye Degisimi

Iki mod vardir (choice basina biri secilir):

**Direct**: Seviyeyi dogrudan hedef enum'a atar.
- Ornek: `vandalismTargetLevel = Heavy` → vandalizm aninda Heavy olur

**Relative**: Mevcut seviyeyi +/- tik kaydirır.
- Light=1, Moderate=2, Heavy=3, Severe=4
- Ornek: Light'tayken +2 → Heavy'ye gecer
- Sonuc < 1 → Ended (vandalizm bastirildi)
- Sonuc > 4 → Severe'de kalir (tavan)

### Periyodik Hasar

Aktif vandalizm seviyelerinde (Light-Severe) her `vandalismDamageInterval` saniyede bir wealth kaybi uygulanir:

| Seviye | Tick Basina Kayip |
|--------|------------------|
| Light | `vandalismLightDamage` (5) |
| Moderate | `vandalismModerateDamage` (15) |
| Heavy | `vandalismHeavyDamage` (30) |
| Severe | `vandalismSevereDamage` (50) |

None ve Ended seviyelerinde hasar uygulanmaz.

### Inspector Kullanimi

Choice'un "Vandalizm Etkisi" foldout'u acilir:
1. `Vandalizmi Etkiler` tiklanir
2. `Degisim Tipi` secilir (Direct veya Relative)
3. Direct ise `Hedef Seviye` secilir, Relative ise `Seviye Degisimi (+/-)` girilir

### Savas Basi / Sonu

- Savas basladiginda `currentVandalismLevel = None`, hasar timer sifirlanir
- Savas sonucunda `finalVandalismLevel` kaydedilir (UI gosterebilir)

### Eventler

| Event | Parametre | Ne Zaman |
|-------|-----------|----------|
| `OnVandalismLevelChanged` | VandalismLevel | Vandalizm seviyesi degisti |
| `OnVandalismDamage` | float | Hasar tick'i uygulandı (miktar) |

---

## Formuller

### Baski Basari Sansi

```
successChance = clamp(politicalInfluence * politicalInfluenceMultiplier, 0, 0.95)
```

- Siyasi nufuz 0 veya negatifse → %0 sans
- Siyasi nufuz 95+ (carpan 0.01 ile) → %95 sans (tavan)

### Savas Kazanma Sansi

```
winChance = clamp(baseWinChance - invasionDifficulty + (supportStat / 100) * supportWinBonus, minWinChance, maxWinChance)
```

| Senaryo | invasionDifficulty | supportStat | Sonuc |
|---------|--------------------|-------------|-------|
| Kolay ulke, tam destek | 0.1 | 100 | **%90** |
| Zor ulke, dusuk destek | 0.4 | 20 | **%10** |
| Orta ulke, orta destek | 0.25 | 50 | **%43.75** |

### Normal Kazanma Odulu

```
effectiveBaseReward = baseReward + bonusRewards[country]
reward = effectiveBaseReward * rewardMultiplier * (supportStat / 100) * supportRewardRatio - accumulatedCostModifier
```

- `rewardMultiplier`: Event'lerdeki `baseRewardReduction` secenekleriyle carpilarak azalir (1.0'dan baslar)
- `supportRewardRatio` (0.8): Support 100 olsa bile baseReward'in max %80'i alinir

### Anlasma Odulu

```
reward = effectiveBaseReward * rewardMultiplier * dealRewardRatio - accumulatedCostModifier
```

Zar atilmaz, garanti odul verilir.

### Kose Kapma Odulu (savas kazanilirsa)

```
grabRatio = cornerGrabStat / 100
reward = effectiveBaseReward * rewardMultiplier * grabRatio * supportRewardRatio - accumulatedCostModifier
rivalShare = effectiveBaseReward * (1 - grabRatio)  → rakip ulkenin bonusRewards'una eklenir
```

### Kaybetme Cezasi

```
wealthChange = -(warLossPenalty + accumulatedCostModifier)
suspicionChange = warLossSuspicionIncrease + accumulatedSuspicionModifier
politicalInfluenceChange = -warLossPoliticalPenalty + accumulatedPoliticalInfluenceModifier
```

- Savas kaybedilirse **minigame kalici olarak kapanir** (bir daha oynamaz)
- Zincir cokusu de kayip sayilir

### Ateskes

Savas sirasinda oyuncu `supportStat >= ceasefireMinSupport` ise ateskes talep edebilir.

```
ratio = (supportStat - ceasefireMinSupport) / (100 - ceasefireMinSupport)
wealthChange = lerp(-ceasefirePenalty, effectiveBaseReward * rewardMultiplier * ceasefireMaxReward, ratio) - accumulatedCostModifier
```

| supportStat | ratio | Sonuc |
|-------------|-------|-------|
| 40 | 0.0 | Zararli |
| 70 | 0.5 | Basabas |
| 100 | 1.0 | Max kar |

**Kurallar:**
- supportStat < 40 → ateskes **kullanilamaz**
- Ulke fethedilmez (tekrar saldiriabilir)
- Minigame **kapanmaz**
- Cooldown baslar

### Zincir Ateskesi

Zincir sirasinda `triggersCeasefire` flagli choice secilirse ateskes tetiklenir. Normal ateskesten farki:
- `ceasefireMinSupport` kontrolu **YAPILMAZ**
- ratio = `supportStat / 100` (tam aralik kullanilir)

### Toplum Tepkisi Ateskesi

Toplum tepkisi stat'i `protestFailThreshold`'u astiginda otomatik tetiklenir. Normal ateskes formulunu kullanir ancak:
- Oyuncu **talep etmez**, otomatik tetiklenir
- `wasProtestCeasefire = true` olarak isaretlenir
- `ceasefireMinSupport` kontrolu yapar (ratio negatife dusmemesi icin clamp edilir)

---

## Ulke Rotasyonu

UI'da ayni anda `visibleCountryCount` (varsayilan 3) ulke gosterilir. Her `rotationInterval` (varsayilan 90 sn) saniyede bir tanesi degistirilir.

**Kurallar:**
- Iste giren her ulke en az 1 rotasyon suresi boyunca korunur (swap edilemez)
- Secili ulke (aktif savas/baski) swap edilemez
- Isgal edilmis (conquered) ulkeler havuzdan cikarilir
- Havuzda yeterli ulke yoksa swap yapilmaz
- UI acip kapatmak listeyi degistirmez — rotasyon state'ten bagimsiz calisir

---

## Manager API

### UI'in Cagirdigi Metodlar

| Metod | Parametre | Ne Yapar |
|-------|-----------|----------|
| `SelectCountry(country)` | WarForOilCountry | Ulke secip PressurePhase'e gecer. Unlock, cooldown, conquered kontrolleri yapar. |
| `AttemptPressure()` | — | Baski denemesi. Basarili → savas baslar. Basarisiz → cooldown. |
| `CancelPressure()` | — | Baskidan vazgecip Idle'a doner. |
| `ResolveEvent(choiceIndex)` | int | Event secimi yapar, modifier'lari uygular. Zincir/rakip/normal akisa gore dallanir. |
| `RequestCeasefire()` | — | Ateskes talep eder. supportStat >= minSupport gerekir. |
| `DismissResultScreen()` | — | Sonuc ekranini kapatir, stat'lari uygular, cooldown baslatir. |

### Events (UI Dinleyecek)

| Event | Parametre | Ne Zaman |
|-------|-----------|----------|
| `OnCountrySelected` | WarForOilCountry | Ulke secildi |
| `OnPressureResult` | bool, float | Baski sonucu (basari, cooldown suresi) |
| `OnPressureCooldownUpdate` | float | Cooldown geri sayimi (her frame) |
| `OnWarStarted` | WarForOilCountry, float | Savas baslamis (ulke, sure) |
| `OnWarProgress` | float | Savas ilerlemesi (0-1) |
| `OnWarEventTriggered` | WarForOilEvent | Event tetiklendi |
| `OnEventDecisionTimerUpdate` | float | Event karar sayaci |
| `OnWarEventResolved` | WarForOilEventChoice | Secim yapildi |
| `OnCeasefireResult` | WarForOilResult | Ateskes sonucu |
| `OnWarResultReady` | WarForOilResult | Sonuc hazir, ekran goster |
| `OnWarFinished` | WarForOilResult | Sonuc ekrani kapandi, her sey bitti |
| `OnActiveCountriesChanged` | List\<WarForOilCountry\> | Ulke listesi degisti |
| `OnChainStarted` | — | Zincir basladi (UI savas timer'i dondurabilir) |
| `OnChainEnded` | string | Zincir bitti (sebep: "collapse", "ceasefire", "government_collapse") |
| `OnRivalInvasionStarted` | WarForOilCountry | Rakip isgal tetiklendi (rakip ulke bilgisi) |
| `OnCornerGrabStarted` | — | Kose kapma yarisi basladi (anlasma reddedildi) |
| `OnCornerGrabStatChanged` | float | Kose kapma stat'i degisti (0-100) |
| `OnProtestForeshadow` | — | Feed savas karsiti gonderilere dondu (foreshadowing) |
| `OnProtestStarted` | — | Toplum tepkisi basladi |
| `OnProtestStatChanged` | float | Toplum tepkisi stat'i degisti (0-100) |
| `OnProtestSuppressed` | — | Toplum tepkisi basariyla bastirildi |
| `OnVandalismLevelChanged` | VandalismLevel | Vandalizm seviyesi degisti |
| `OnVandalismDamage` | float | Vandalizm hasar tick'i (miktar) |

### Getter'lar

| Metod | Donus | Aciklama |
|-------|-------|----------|
| `IsActive()` | bool | Minigame aktif mi (Idle degilse true) |
| `IsPermanentlyDisabled()` | bool | Kalici devre disi mi |
| `IsCountryConquered(country)` | bool | Ulke isgal edilmis mi |
| `CanRequestCeasefire()` | bool | Ateskes talep edilebilir mi |
| `IsInChain()` | bool | Zincir aktif mi |
| `IsCornerGrabRace()` | bool | Kose kapma yarisi aktif mi |
| `GetCurrentState()` | WarForOilState | Mevcut durum |
| `GetSelectedCountry()` | WarForOilCountry | Secili ulke |
| `GetSupportStat()` | float | Destek degeri |
| `GetCornerGrabStat()` | float | Kose kapma stat degeri (0-100) |
| `GetRivalCountry()` | WarForOilCountry | Rakip ulke (varsa) |
| `GetBonusReward(country)` | float | Ulkenin bonus reward'i |
| `GetActiveCountries()` | List\<WarForOilCountry\> | UI'daki ulke listesi |
| `GetWarProgress()` | float | Savas ilerlemesi (0-1) |
| `IsProtestActive()` | bool | Toplum tepkisi aktif mi |
| `GetProtestStat()` | float | Toplum tepkisi degeri (0-100) |
| `GetVandalismLevel()` | VandalismLevel | Mevcut vandalizm seviyesi |

---

## Oyun Duraklama Davranisi

| Durum | Oyun Durumu | Timer Tipi |
|-------|-------------|------------|
| WarProcess | **Devam ediyor** | `Time.deltaTime` |
| EventPhase | **Duraklatilmis** | `Time.unscaledDeltaTime` |
| ChainWaiting | **Devam ediyor** | `Time.deltaTime` (savas timer durur, chain timer calisir) |
| ResultPhase | **Duraklatilmis** | Timer yok (UI bekleniyor) |
| PressurePhase | **Devam ediyor** | `Time.deltaTime` |

---

## EventCoordinator Entegrasyonu

### Cooldown (Kisa Sureli)
Savas sirasinda event tetiklemeden once `EventCoordinator.CanShowEvent()` kontrol edilir. Event tetiklendiginde `EventCoordinator.MarkEventShown()` cagirilir → diger sistemler kisa bir cooldown suresince event gonderemez.

### Lock (Uzun Sureli)
Zincir basladiginda `EventCoordinator.LockEvents("WarForOilChain")` cagirilir — tum zincir boyunca diger sistemlerin event gondermesi engellenir. Zincir bittiginde `EventCoordinator.UnlockEvents("WarForOilChain")` ile serbest birakilir.

---

## Inspector (Custom Editor)

`WarForOilEventEditor.cs` dosyasi Inspector'da kolay event duzenleme saglar:

- **ChainRole'e gore alan gosterimi**: Head secilince tum zincir config'i (nextChainEvent, chainInterval, skillsToLock, chainFine, refusalThresholds) gosterilir. Link secilince sadece nextChainEvent ve chainInterval gosterilir. None secilince hicbir zincir alani gosterilmez.
- **isRepeatable tiklenince** maxRepeatCount gosterilir
- **Choice'lar foldout ile**: Her choice icinde 7 foldout grubu:
  1. **Modifiers** — supportModifier, suspicionModifier, reputationModifier, politicalInfluenceModifier, costModifier, cornerGrabModifier, protestModifier, olasilikli tepki
  2. **Diger Sonuclar** — endsWar, reducesReward, endsWarWithDeal, blocksEvents, freezesFeed, hasFeedOverride (alt kosullu alanlarla)
  3. **Zincir Flagleri** — continuesChain, isChainRefusal, triggersCeasefire
  4. **Rakip Isgal Flagleri** — acceptsRivalDeal, rejectsRivalDeal
  5. **Vandalizm Etkisi** — affectsVandalism, vandalismChangeType, vandalismTargetLevel/vandalismLevelDelta (kosullu)
  6. **On Kosullar** — requiredSkills, statConditions

---

## Tipik Oyun Akislari

### Normal Savas

1. **Rotasyon calisir** — UI'da 3 ulke gosterilir
2. **Oyuncu ulke secer** → PressurePhase
3. **Oyuncu baski yapar** → basarisizsa cooldown, basariliysa savas baslar
4. **Savas sureci** — 5 dakika, her 15 sn'de event kontrolu
5. **Event gelir** → oyun durur, oyuncu secer → modifier'lar uygulanir
6. **Savas biter** → olasilik kontrolu → kazanma/kaybetme
7. **Sonuc ekrani** → stat'lar uygulanir

### Zincir Event Akisi

1. Normal savas sirasinda Head event tetiklenir
2. Zincir baslar → savas timer durur, diger event'ler kilitleni
3. Oyuncu fonlar (continuesChain) veya reddeder (isChainRefusal)
4. chainInterval sonra sonraki Link event gelir
5. Son event'e kadar devam edilirse → hukumet dususu (skill lock, ceza yok)
6. Fazla reddedilirse → zincir cokusu (skill lock + chainFine cezasi)
7. triggersCeasefire secilirse → ateskes (minSupport kontrolu yok)

### Rakip Isgal Akisi

1. Savas 60+ saniye surdukten sonra %30 ihtimalle rakip isgal tetiklenir
2. Rakip ulke havuzdan secilir, `rivalOfferEvent` gosterilir
3. **Kabul** → %60 odul oyuncuya, %40 rakip ulkeye eklenir, savas hizla biter, **event gelmez**
4. **Red** → kose kapma yarisi baslar, eventler cornerGrabEvents'ten gelir
5. cornerGrabStat event secimlerine gore degisir
6. Savas sonunda cornerGrabStat'a gore odul bolunur

### Toplum Tepkisi Akisi

1. Savas 90+ saniye surdukten sonra %25 ihtimalle toplum tepkisi tetiklenir
2. **Faz 1** — `OnProtestForeshadow` tetiklenir, feed savas karsiti gonderilere doner, event gosterilmez
3. **Faz 2** — Bir sonraki event check'te `protestTriggerEvent` gosterilir, `protestStat` 30'dan baslar
4. Bundan sonra normal + protest event havuzlari birlestirilir, her check'te ikisinden biri gelebilir
5. Oyuncu choice'larla `protestStat`'i etkiler — her choice ayrica sonraki drift hizini belirler
6. Pasif drift: her 3 saniyede son modifier / 10 kadar stat kayar (+3 choice → +0.3/tick, -5 choice → -0.5/tick)
7. **Basarisizlik**: protestStat >= 80 → otomatik ateskes (`wasProtestCeasefire = true`)
8. **Basari**: protestStat < 10 → tepki bastirildi, protest eventleri durur, savas normal devam eder

---

## Dosya Yapisi

```
Assets/Scripts/Minigames/WarForOil/
├── WarForOilCountry.cs         — ulke verisi (ScriptableObject)
├── WarForOilEvent.cs           — event + choice + ChainRole + RefusalThreshold
├── WarForOilDatabase.cs        — ayarlar + event havuzlari (ScriptableObject)
├── WarForOilManager.cs         — ana mantik + state machine + rotasyon (MonoBehaviour)
├── Editor/
│   └── WarForOilEventEditor.cs — Inspector custom editor
└── warforoil-readme.md         — bu dosya
```
