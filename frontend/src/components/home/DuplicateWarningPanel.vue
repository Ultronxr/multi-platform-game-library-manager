<script setup lang="ts">
import type { DuplicateGroup } from "../../types/gameLibrary";

interface DuplicateWarningPanelProps {
  duplicates: DuplicateGroup[];
}

const props = defineProps<DuplicateWarningPanelProps>();
</script>

<template>
  <a-card title="重复购买预警" :bordered="false" class="surface-card">
    <a-empty v-if="props.duplicates.length === 0" description="当前未发现重复购买组" />
    <a-collapse v-else ghost>
      <a-collapse-panel
        v-for="group in props.duplicates"
        :key="group.normalizedTitle"
        :header="group.games[0]?.title || group.normalizedTitle"
      >
        <a-list :data-source="group.games" size="small">
          <template #renderItem="{ item }">
            <a-list-item>
              <a-space>
                <a-tag color="geekblue">{{ item.platform }}</a-tag>
                <span>{{ item.accountName }}</span>
                <a-typography-text code>{{ item.externalId }}</a-typography-text>
              </a-space>
            </a-list-item>
          </template>
        </a-list>
      </a-collapse-panel>
    </a-collapse>
  </a-card>
</template>
