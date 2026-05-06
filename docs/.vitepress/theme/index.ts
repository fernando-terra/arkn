import { h } from 'vue'
import DefaultTheme from 'vitepress/theme'
import NuGetStats from './components/NuGetStats.vue'
import './style.css'

export default {
  extends: DefaultTheme,
  Layout() {
    return h(DefaultTheme.Layout, null, {
      // Renders below the features grid on any page with layout: home
      'home-features-after': () => h(NuGetStats),
    })
  },
}
