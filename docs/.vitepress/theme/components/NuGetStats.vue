<template>
  <section v-if="packages.length > 0" class="nuget-stats">
    <div class="nuget-stats__container">
      <div class="nuget-stats__header">
        <span class="nuget-stats__badge">NuGet</span>
        <h2 class="nuget-stats__title">Download stats</h2>
        <p class="nuget-stats__sub">Live data from nuget.org · v0.3.0</p>
      </div>
      <div class="nuget-stats__grid">
        <a
          v-for="pkg in packages"
          :key="pkg.id"
          :href="`https://www.nuget.org/packages/${pkg.id}`"
          target="_blank"
          rel="noopener noreferrer"
          class="nuget-stats__card"
        >
          <span class="nuget-stats__pkg-name">{{ pkg.id }}</span>
          <span class="nuget-stats__downloads">
            <svg class="nuget-stats__dl-icon" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.5">
              <path d="M8 2v8m0 0-3-3m3 3 3-3M2 13h12" stroke-linecap="round" stroke-linejoin="round"/>
            </svg>
            {{ formatDownloads(pkg.totalDownloads) }}
          </span>
        </a>
      </div>
      <p class="nuget-stats__footnote">
        Total across all versions ·
        <a href="https://www.nuget.org/packages?q=Arkn" target="_blank" rel="noopener">
          View all on NuGet →
        </a>
      </p>
    </div>
  </section>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'

interface NuGetPackage {
  id: string
  totalDownloads: number
}

const packages = ref<NuGetPackage[]>([])

function formatDownloads(n: number): string {
  if (n >= 1_000_000) return `${(n / 1_000_000).toFixed(1)}M`
  if (n >= 1_000)     return `${(n / 1_000).toFixed(1)}K`
  return n.toString()
}

onMounted(async () => {
  try {
    const res  = await fetch(
      'https://azuresearch-usnc.nuget.org/query?q=Arkn&take=20&prerelease=false'
    )
    if (!res.ok) return
    const data = await res.json()

    const ARKN_PACKAGES = new Set([
      'Arkn.Core', 'Arkn.Results', 'Arkn.Http', 'Arkn.Jobs',
      'Arkn.Logging', 'Arkn.Notifications', 'Arkn.Analyzers',
      'Arkn.SourceGen', 'Arkn.Templates', 'Arkn.MCP',
      'Arkn.Extensions.Notifications.Slack', 'Arkn.Extensions.Notifications.Email',
      'Arkn.Extensions.Notifications.Teams', 'Arkn.Extensions.Notifications.Discord',
      'Arkn.Extensions.Logging.ApplicationInsights',
      'Arkn.Extensions.Logging.Seq', 'Arkn.Extensions.Logging.Elasticsearch',
    ])

    packages.value = (data.data as any[])
      .filter(p => ARKN_PACKAGES.has(p.id as string))
      .map(p => ({ id: p.id as string, totalDownloads: p.totalDownloads as number }))
      .sort((a, b) => b.totalDownloads - a.totalDownloads)
  } catch {
    // non-critical — silently skip if fetch fails (SSR, offline, CORS)
  }
})
</script>

<style scoped>
.nuget-stats {
  padding: 3rem 0 4rem;
  border-top: 1px solid var(--vp-c-divider);
}

.nuget-stats__container {
  max-width: 1152px;
  margin: 0 auto;
  padding: 0 24px;
}

.nuget-stats__header {
  text-align: center;
  margin-bottom: 2rem;
}

.nuget-stats__badge {
  display: inline-block;
  background: rgba(124, 106, 247, 0.12);
  border: 1px solid rgba(124, 106, 247, 0.25);
  color: var(--vp-c-brand-2, #a78bfa);
  border-radius: 100px;
  padding: 0.2rem 0.75rem;
  font-size: 0.75rem;
  font-weight: 600;
  letter-spacing: 0.04em;
  text-transform: uppercase;
  margin-bottom: 0.75rem;
}

.nuget-stats__title {
  font-size: 1.5rem;
  font-weight: 700;
  letter-spacing: -0.02em;
  margin: 0 0 0.4rem;
  border: none;
  padding: 0;
}

.nuget-stats__sub {
  font-size: 0.9rem;
  color: var(--vp-c-text-2);
  margin: 0;
}

.nuget-stats__grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(220px, 1fr));
  gap: 0.75rem;
  margin-bottom: 1.25rem;
}

.nuget-stats__card {
  display: flex;
  flex-direction: column;
  gap: 0.4rem;
  padding: 0.9rem 1.1rem;
  background: var(--vp-c-bg-soft);
  border: 1px solid var(--vp-c-divider);
  border-radius: 8px;
  text-decoration: none;
  transition: border-color 0.15s, background 0.15s;
}

.nuget-stats__card:hover {
  border-color: var(--vp-c-brand-1, #7c6af7);
  background: var(--vp-c-bg-elv);
  text-decoration: none;
}

.nuget-stats__pkg-name {
  font-size: 0.82rem;
  font-family: var(--vp-font-family-mono);
  color: var(--vp-c-text-1);
  font-weight: 500;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.nuget-stats__downloads {
  display: flex;
  align-items: center;
  gap: 0.3rem;
  font-size: 1rem;
  font-weight: 700;
  color: var(--vp-c-brand-2, #a78bfa);
}

.nuget-stats__dl-icon {
  width: 14px;
  height: 14px;
  flex-shrink: 0;
}

.nuget-stats__footnote {
  text-align: center;
  font-size: 0.8rem;
  color: var(--vp-c-text-3);
  margin: 0;
}

.nuget-stats__footnote a {
  color: var(--vp-c-brand-2, #a78bfa);
  text-decoration: none;
}

.nuget-stats__footnote a:hover {
  text-decoration: underline;
}

@media (max-width: 640px) {
  .nuget-stats__grid {
    grid-template-columns: repeat(auto-fill, minmax(160px, 1fr));
  }
}
</style>
