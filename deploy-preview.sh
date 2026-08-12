#!/usr/bin/env bash
# =============================================================================
# Berex Tech QMS — Local Preview Deployment
# =============================================================================
# One-command deployment for reviewing the complete QMS system locally.
#
# Prerequisites:
#   - Docker Desktop (or Docker Engine + Docker Compose v2)
#   - Ports 3000, 5000, 5432, 6379, 9000 available
#
# Usage:
#   chmod +x deploy-preview.sh
#   ./deploy-preview.sh
#
# Access:
#   Web UI:  http://localhost:3000
#   API:     http://localhost:5000/health
#   Swagger: http://localhost:5000/swagger
#
# Demo Credentials:
#   admin@berextech.com / Admin@123456  (System Administrator — full access)
# =============================================================================

set -euo pipefail

RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
NC='\033[0m'

echo ""
echo -e "${CYAN}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${CYAN}║          ${GREEN}Berex Tech QMS — Local Preview Deployment${CYAN}          ║${NC}"
echo -e "${CYAN}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Check prerequisites
command -v docker >/dev/null 2>&1 || { echo -e "${RED}Error: Docker is required but not installed.${NC}"; exit 1; }
docker compose version >/dev/null 2>&1 || docker-compose version >/dev/null 2>&1 || { echo -e "${RED}Error: Docker Compose is required but not installed.${NC}"; exit 1; }

# Determine compose command
COMPOSE_CMD="docker compose"
$COMPOSE_CMD version >/dev/null 2>&1 || COMPOSE_CMD="docker-compose"

# Navigate to project root
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

echo -e "${BLUE}[1/4]${NC} Cleaning up any previous deployment..."
$COMPOSE_CMD -f docker/docker-compose.yml down -v --remove-orphans 2>/dev/null || true

echo -e "${BLUE}[2/4]${NC} Building containers (this may take 3-5 minutes on first run)..."
$COMPOSE_CMD -f docker/docker-compose.yml build --no-cache

echo -e "${BLUE}[3/4]${NC} Starting services..."
$COMPOSE_CMD -f docker/docker-compose.yml up -d

echo -e "${BLUE}[4/4]${NC} Waiting for services to become healthy..."

# Wait for PostgreSQL
echo -n "  PostgreSQL: "
for i in $(seq 1 30); do
    if $COMPOSE_CMD -f docker/docker-compose.yml exec -T postgres pg_isready -U berexqms_app -d berexqms >/dev/null 2>&1; then
        echo -e "${GREEN}✅ Ready${NC}"
        break
    fi
    sleep 2
    echo -n "."
done

# Wait for Redis
echo -n "  Redis:      "
for i in $(seq 1 15); do
    if $COMPOSE_CMD -f docker/docker-compose.yml exec -T redis redis-cli -a 'BerexQms_Redis_2026!' ping >/dev/null 2>&1; then
        echo -e "${GREEN}✅ Ready${NC}"
        break
    fi
    sleep 2
    echo -n "."
done

# Wait for API
echo -n "  API:        "
for i in $(seq 1 30); do
    if curl -sf http://localhost:5000/health >/dev/null 2>&1; then
        echo -e "${GREEN}✅ Ready${NC}"
        break
    fi
    sleep 3
    echo -n "."
done

# Wait for Web
echo -n "  Web UI:     "
for i in $(seq 1 15); do
    if curl -sf http://localhost:3000 >/dev/null 2>&1; then
        echo -e "${GREEN}✅ Ready${NC}"
        break
    fi
    sleep 2
    echo -n "."
done

echo ""
echo -e "${GREEN}╔══════════════════════════════════════════════════════════════╗${NC}"
echo -e "${GREEN}║                   ✅ Deployment Complete!                    ║${NC}"
echo -e "${GREEN}╠══════════════════════════════════════════════════════════════╣${NC}"
echo -e "${GREEN}║                                                              ║${NC}"
echo -e "${GREEN}║${NC}  ${CYAN}Web UI:${NC}   ${YELLOW}http://localhost:3000${NC}                             ${GREEN}║${NC}"
echo -e "${GREEN}║${NC}  ${CYAN}API:${NC}      ${YELLOW}http://localhost:5000/health${NC}                      ${GREEN}║${NC}"
echo -e "${GREEN}║${NC}  ${CYAN}Swagger:${NC}  ${YELLOW}http://localhost:5000/swagger${NC}                     ${GREEN}║${NC}"
echo -e "${GREEN}║                                                              ║${NC}"
echo -e "${GREEN}║${NC}  ${CYAN}Login:${NC}    admin@berextech.com / Admin@123456             ${GREEN}║${NC}"
echo -e "${GREEN}║${NC}           (System Administrator — full access)              ${GREEN}║${NC}"
echo -e "${GREEN}║                                                              ║${NC}"
echo -e "${GREEN}╠══════════════════════════════════════════════════════════════╣${NC}"
echo -e "${GREEN}║${NC}  To stop: ${YELLOW}docker compose -f docker/docker-compose.yml down${NC}   ${GREEN}║${NC}"
echo -e "${GREEN}╚══════════════════════════════════════════════════════════════╝${NC}"
echo ""
